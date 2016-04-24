using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MKVExtractWPF
{
    public class ProgessEventArgs : EventArgs
    {
        public ProgessEventArgs(int p)
        {
            progess = p;
        }
        private int progess;
        public int Progess
        {
            get { return progess; }
        }
    }
    
    public class Utility
    {
        public static void ShowStatus(string status)
        {
            MainWindow.SetStatus(status);
        }
        
        public static event EventHandler<ProgessEventArgs> ProgressUpdate;

        public static void OutputReceived(object sender, DataReceivedEventArgs e)
        {
            string str = e.Data;
            if (str == null)
            {
                return;
            }

            const string SGN = "Progress:";
            int pos = str.IndexOf(SGN);
            if (pos >= 0)
            {
                string s1 = str.Substring(pos + SGN.Length).Trim().TrimEnd('%');
                int progress = 0;
                if (int.TryParse(s1, out progress))
                {
                    ProgressUpdate(null, new ProgessEventArgs(progress));
                    str = string.Format("Extracting tracks: {0}%", progress);
                }
            }
            MainWindow.SetStatus(str);
        }

        /// <span class="code-SummaryComment"><summary></span>
        /// Executes a shell command synchronously.
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="command">string command</param></span>
        /// <span class="code-SummaryComment"><returns>string, as output of the command.</returns></span>
        public static string ExecuteCommand(object command, bool async = false)
        {
            string result = null;

            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + command);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                Process proc = new Process();
                proc.StartInfo = procStartInfo;

                if (async)
                {
                    proc.OutputDataReceived += OutputReceived;
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.WaitForExit();
                    MainWindow.SetStatus("Extracted OK.");
                }
                else
                {
                    proc.Start();

                    // Get the output into a string
                    result = proc.StandardOutput.ReadToEnd();
                }

            }
            catch (Exception objException)
            {
                // Log the exception
                ShowStatus(objException.Message);
            }

            return result;
        }

        private static void AsyncExecuteCommand(object command)
        {
            ExecuteCommand(command, true);
        }

        /// <span class="code-SummaryComment"><summary></span>
        /// Execute the command Asynchronously.
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="command">string command.</param></span>
        public static void ExecuteCommandAsync(string command)
        {
            try
            {
                //Asynchronously start the Thread to process the Execute command request.
                Thread objThread = new Thread(new ParameterizedThreadStart(AsyncExecuteCommand));
                //Make the thread as background thread.
                objThread.IsBackground = true;
                //Set the Priority of the thread.
                objThread.Priority = ThreadPriority.AboveNormal;
                //Start the thread.
                objThread.Start(command);
            }
            catch (ThreadStartException objException)
            {
                // Log the exception
                ShowStatus(objException.Message);
            }
            catch (ThreadAbortException objException)
            {
                // Log the exception
                ShowStatus(objException.Message);
            }
            catch (Exception objException)
            {
                // Log the exception
                ShowStatus(objException.Message);
            }
        }

        public static void CopyToClipboard(string text)
        {
            Clipboard.SetDataObject(text);
            ShowStatus("Command Copied!");
        }
    }
}
