using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Shell;

namespace MKVExtractWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow Current = null;
        private MkvExtractor Extractor = new MkvExtractor();
        public ObservableCollection<Track> Tracks { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Current = this;
            Tracks = new ObservableCollection<Track>();

            TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
            Utility.ProgressUpdate += OnUpdateProgress;
        }

        private void SelectInputFile(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Matroska Files|*.mkv;*.mka";
            ofd.ShowDialog();

            InputFile.Text = ofd.FileName;

            FillFileList();
        }

        private void FillFileList()
        {
            SetStatus("Executing mkvinfo...");
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;

            Tracks.Clear();

            if (!Extractor.LoadFromFile(InputFile.Text))
            {
                SetStatus("Error opening file.");
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Error;
                return;
            }

            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;

            Segment seg0 = Extractor.Info.Segments()["Segment tracks"];
            if (null != seg0)
            {
                for (int i = 0; i < seg0.Count(); i++)
                {
                    Segment seg = seg0[i];
                    string _lang = seg.GetValue("Language");
                    string s = string.Format("Track {0}: {1}, {2} [{3}] {4}",
                        seg.GetValue("Track number"), seg.GetValue("Track type"),
                        seg.GetValue("Codec ID"), _lang, seg.GetValue("Name"));
                    if (seg.GetValue("Track type") == "audio")
                    {
                        Segment segAudio = seg["Audio track"];
                        s += string.Format("({0}ch, {1}hz)",
                            segAudio.GetValue("Channels"), segAudio.GetValue("Sampling frequency"));
                    }
                    Tracks.Add(new Track(s, Track.TrackType.Tracks, i));
                }
            }

            // TO DO: Add attachments

            //seg0 = Extractor.Info.Segments()["Chapters"];
            //if (null != seg0)
            //{
            //    string s = null;
            //    if (seg0.Count() > 1)
            //    {
            //        s = string.Format("Chapters: {0}", seg0.Count());
            //    }
            //    else
            //    {
            //        Segment seg = seg0[0];
            //        int parcnt = 0;
            //        for (int i = 0; i < seg.Count(); i++)
            //        {
            //            if (seg[i].Name != "ChapterAtom")
            //            {
            //                parcnt++;
            //            }
            //        }
            //        s = string.Format("Chapters: {0}", seg.Count() - parcnt);
            //    }
            //    Tracks.Add(new Track(s, Track.TrackType.Attachments, 0));
            //}

            SetStatus("");
        }

        private void SelectOutDir(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Select Output Dir:";
            fbd.SelectedPath = OutputDir.Text;
            fbd.ShowDialog();

            OutputDir.Text = fbd.SelectedPath;
        }

        private void UseInputForOutput_Checked(object sender, RoutedEventArgs e)
        {
            if (UseInputForOutput.IsChecked.Value && InputFile.Text.Count() != 0)
            {
                OutputDir.Text = Path.GetDirectoryName(InputFile.Text);
            }
        }

        private void ExtractFile(object sender, RoutedEventArgs e)
        {
            DoExtract();
        }

        static readonly string[] MODE_LIST = new string[] { "tracks", "attachments", "chapters" };

        private string DoExtract(bool simulate = false)
        {
            Extractor.OGM_Chapters = (ChapterFormat.SelectedIndex == 1);

            string path = null;
            if (UseInputForOutput.IsChecked.Value)
            {
                if (InputFile.Text.Count() != 0)
                {
                    path = Path.GetDirectoryName(InputFile.Text);
                }
            }
            else
            {
                path = OutputDir.Text;
            }

            if (null == path)
            {
                return "";
            }

            Dictionary<string, List<int>> taskList = new Dictionary<string, List<int>>();

            for (int i = 0; i < FileInfo.Items.Count; i++)
            {
                Track tr = Tracks[i];
                if (tr.IsChecked)
                {
                    List<int> task = null;
                    if (!taskList.TryGetValue(tr.TypeName, out task))
                    {
                        taskList.Add(tr.TypeName, new List<int>());
                    }

                    taskList[tr.TypeName].Add(tr.Index);
                }
            }

            string cmd = "";
            foreach (string type in taskList.Keys)
            {
                if (taskList[type].Count != 0)
                {
                    cmd = Extractor.GetExtractCMD(type, taskList[type], path);
                    if (!simulate)
                    {
                        SetStatus("Extraction queued...");
                        TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                        Utility.ExecuteCommandAsync(cmd);
                    }
                }
            }
            return cmd;
        }

        public void OnUpdateProgress(object sender, ProgessEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => TaskbarItemInfo.ProgressValue = e.Progess / (double) 100));
        }

        public static void SetStatus(string text)
        {
            Current.Dispatcher.BeginInvoke(new Action(() => Current.StatusBar.Text = text));
        }

        private void BatchCommand(object sender, RoutedEventArgs e)
        {
            string cmd = DoExtract(true);
            if (!string.IsNullOrEmpty(cmd))
            {
                Utility.CopyToClipboard(cmd);
            }
        }

        private void UncheckAll(object sender, RoutedEventArgs e)
        {
            foreach (Track track in Tracks)
            {
                track.IsChecked = false;
            }
        }

        private void CheckAll(object sender, RoutedEventArgs e)
        {
            foreach (Track track in Tracks)
            {
                track.IsChecked = true;
            }
        }
    }
}
