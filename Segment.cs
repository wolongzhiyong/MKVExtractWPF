using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Specialized;

namespace MKVExtractWPF
{
    public class Segment
    {
        public string Name { get; set; }
        public StringDictionary Strings { get; set; }
        public List<Segment> SubSegements { get; set; }

        public Segment()
        {
            Strings = new StringDictionary();
            SubSegements = new List<Segment>();
        }

        public Segment(string text)
        {
            Strings = new StringDictionary();
            SubSegements = new List<Segment>();
            ParseFromText(text);
        }

        ~Segment()
        {
            Clear();
        }

        private void AddSubSegment(string name, string text)
        {
            Segment seg = new Segment(text);
            seg.Name = name;
            SubSegements.Add(seg);

            int indexComma = name.IndexOf(": ");
            int indexBracket = name.IndexOf('(');
            if (indexComma != -1 && (indexBracket == -1 || indexBracket > indexComma))
            {
                Strings.Add(name.Substring(0, indexComma), name.Substring(indexComma+2));
            }
        }

        private Segment Get(int index)
        {
            return SubSegements[index];
        }

        private Segment Get(string name)
        {
            for (int i = 0; i < SubSegements.Count; i++)
            {
                if (SubSegements[i].Name == name)
                {
                    return SubSegements[i];
                }
            }
            return null;
        }

        public string GetValue(string name)
        {
            return Strings[name];
        }

        public Segment this[int index]
        {
            get
            {
                return Get(index);
            }
        }

        public Segment this[string name]
        {
            get
            {
                return Get(name);
            }
        }

        public void Clear()
        {
            SubSegements.Clear();
        }

        public int Count()
        {
            return SubSegements.Count;
        }

        public bool ParseFromText(string text)
        {
            Clear();
            text = text.Trim();
            if (text.Count() == 0)
            {
                return false;
            }

            StringCollection sc = new StringCollection();
            sc.AddRange(text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            sc.Add(sc[0]); // enclosing '+'
            int clevel = sc[0].IndexOf('+');
            string tmp = "";
            string tmpn = "";
            for (int i = 0; i < sc.Count; i++)
            {
                string s = sc[i].Trim();
                if (s.Count() == 0)
                {
                    continue;
                }

                int level = s.IndexOf('+');
                if (level == clevel)
                {
                    if (tmpn != "")
                    {
                        AddSubSegment(ExtString(tmpn), tmp);
                        tmp = "";
                    }
                    tmpn = ExtString(s);
                }
                else if (level > clevel)
                {
                    tmp = tmp + "\r\n" + s;
                }
            }

            return true;
        }

        public static string ExtString(string infoLine)
        {
            int pos = infoLine.IndexOf('+');
            return infoLine.Substring(pos + 1).Trim();
        }
    }
}
