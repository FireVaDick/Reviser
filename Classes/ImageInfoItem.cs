using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Reviser
{
    public class ImageInfoItem
    {
        #region Свойства
        public int Index { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Resolution { get; set; }
        public string AspectRatio { get; set; }
        public string FileSize { get; set; }
        public string Format { get; set; }
        public string Character { get; set; }
        public string Author { get; set; }
        public string Class { get; set; }
        public string Number { get; set; }
        public string Tags { get; set; }

        public List<string> CharacterList { get; set; } = new List<string>();
        public List<string> AuthorList { get; set; } = new List<string>();
        public List<string> TagList { get; set; } = new List<string>();

        public string BackColor
        {
            get
            {
                switch (Class)
                {
                    case "1s": return "#001"; 
                    case "2e": return "#002";
                    case "3n": return "#003";
                    case "4c": return "#004";
                    case "5j": return "#005";
                    case "6h": return "#006";
                    default: return "#000";
                };
            }
        }
        public double IndicatorWidth
        {
            get
            {
                double ratio = Convert.ToDouble(AspectRatio);
                double maxWidth = 16; 

                if (ratio <= 1.0) 
                    return maxWidth;
                else return maxWidth / ratio;
            }
        }

        public double IndicatorHeight
        {
            get
            {
                double ratio = Convert.ToDouble(AspectRatio);
                double maxHeight = 16; 

                if (ratio >= 1.0) 
                    return maxHeight;
                else return maxHeight * ratio;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
