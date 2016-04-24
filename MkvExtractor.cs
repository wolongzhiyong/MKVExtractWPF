using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKVExtractWPF
{
    public class MkvInfo : Segment
    {
        public MkvInfo()
        {
            Name = "root";
        }

        public Segment Head()
        {
            return SubSegements[0];
        }

        public Segment Segments()
        {
            return SubSegements[1];
        }
    }


    public class MkvExtractor
    {
        private string _MKVFileName;

        public bool OGM_Chapters { get; set; }
        public MkvInfo Info { get; set; }

        public MkvExtractor()
        {
            OGM_Chapters = false;
            Info = new MkvInfo();
        }

        public bool LoadFromFile(string filename)
        {
            if (!System.IO.File.Exists(filename))
            {
                return false;
            }

            _MKVFileName = "";
            string command = string.Format("{0}mkvinfo.exe -G --ui-language en {1}", mkvtoolnixPath, filename);
            string txt = Utility.ExecuteCommand(command);
            if (Info.ParseFromText(txt))
            {
                if (Info.Count() > 0)
                {
                    _MKVFileName = filename;
                    return true;
                }
            }
            return false;
        }

        public string GetExtractCMD(string mode, List<int> tracks, string outpath)
        {
            if (tracks.Count == 0)
            {
                return null;
            }

            string mkvex = string.Format("{0}mkvextract --ui-language en", mkvtoolnixPath);
            string cmd = string.Format("{0} {1} \"{2}\"", mkvex, mode, _MKVFileName);
            for (int i = 0; i < tracks.Count; i++)
            {
                string fileName = "";
                int trackID = tracks[i];
                if (mode == "tracks")
                {
                    Segment seg = Info.Segments()["Segment tracks"][trackID];
                    string codec_id = seg.GetValue("Codec ID");
                    string lang = seg.GetValue("Language");
                    string trLang = lang != "" ? lang : "eng";
                    fileName = string.Format("{0}_track{1}_{2}.{3}", Path.GetFileNameWithoutExtension(_MKVFileName), trackID + 1, trLang, GetCodecExtByID(codec_id));
                }
                else if (mode == "attachments")
                {
                    Segment seg = Info.Segments()["Attachments"][trackID];
                    fileName = seg.GetValue("File name");
                }

                if (fileName != "")
                {
                    cmd += string.Format(" {0}:\"{1}\"", trackID, Path.Combine(outpath, fileName));
                }
            }
            
            return cmd;
        }

        static readonly StringDictionary CODEC_EXT = new StringDictionary()
        {
            { "V_MPEG1","mpg"         },
            { "A_AAC","aac"           },
            { "A_AC3","ac3"           },
            { "A_DTS","dts"           },
            { "A_FLAC","flac"         },
            { "A_APE","ape"           },
            { "A_QUICKTIME","qdm"     },
            { "A_TTA1","tta"          },
            { "A_WAVPACK4","wv"       },
            { "A_VORBIS","ogg"        },
            { "A_REAL","ra"           },
            { "V_MPEG2","mpg"         },
            { "V_REAL","rmvb"         },
            { "V_MS/VFW/FOURCC","avi" },
            { "V_MPEG4/ISO/AVC","h264"},
            { "S_VOBSUB","idx"        },
            { "A_MPEG/L3","mp3"       },
            { "A_MPEG/L2","mp2"       },
            { "A_MPEG/L1","mpa"       },
            { "A_PCM/INT/LIT","wav"   },
            { "S_HDMV/PGS","sup"      },
            { "S_TEXT/UTF8","srt"     },
            { "S_TEXT/SSA","ssa"      },
            { "S_TEXT/ASS","ass"      },
            { "S_TEXT/USF","usf"      }
        };

        public static string GetCodecExtByID(string ID)
        {
            if (CODEC_EXT.ContainsKey(ID))
                return CODEC_EXT[ID];
            else
                return null;
        }

        const string mkvtoolnixPath = @"D:\Downloads\Utility\Media\mkvtoolnix\";
    }
}
