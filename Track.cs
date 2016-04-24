using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MKVExtractWPF
{
    public class Track : DependencyObject
    {
        public enum TrackType
        {
            Tracks,
            Attachments,
            Chapters
        }

        public Track(string name, TrackType type, int index)
        {
            DisplayName = name;
            Type = type;
            Index = index;
            IsChecked = false;
        }

        public string DisplayName { get; set; }
        public TrackType Type { get; set; }
        public int Index { get; set; }
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsChecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(Track), new PropertyMetadata(false));
                
        public string TypeName
        {
            get
            {
                string name = "";
                switch (Type)
                {
                    case TrackType.Tracks:
                        name = "tracks";
                        break;
                    case TrackType.Attachments:
                        name = "attachments";
                        break;
                    case TrackType.Chapters:
                        name = "chapters";
                        break;
                    default:
                        break;
                }
                return name;
            } 
        }
    }
}
