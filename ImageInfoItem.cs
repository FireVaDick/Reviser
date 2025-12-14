using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Reviser
{
    public class ImageInfoItem
    {
        #region Свойства
        private int index;
        private string fileName;
        private string filePath;
        private ImageSource previewImageSource;

        private string character;
        private string author;
        private string classs;
        private string number;
        private string tags;

        public string Resolution { get; set; }
        public string AspectRatio { get; set; }
        public string FileSize { get; set; }
        public string Format { get; set; }

        public int Index
        {
            get => index;
            set
            {
                index = value;
                OnPropertyChanged();
            }
        }

        public string FileName
        {
            get => fileName;
            set
            {
                fileName = value;
                OnPropertyChanged();
            }
        }

        public string FilePath
        {
            get => filePath;
            set
            {
                filePath = value;
                OnPropertyChanged();
            }
        }

        public ImageSource PreviewImageSource
        {
            get => previewImageSource;
            set
            {
                previewImageSource = value;
                OnPropertyChanged();
            }
        }

        public string Character
        {
            get => character;
            set
            {
                character = value;
                OnPropertyChanged();
            }
        }

        public string Author
        {
            get => author;
            set
            {
                author = value;
                OnPropertyChanged();
            }
        }

        public string Class
        {
            get => classs;
            set
            {
                classs = value;
                OnPropertyChanged();
            }
        }

        public string Number
        {
            get => number;
            set
            {
                number = value;
                OnPropertyChanged();
            }
        }

        public string Tags
        {
            get => tags;
            set
            {
                tags = value;
                OnPropertyChanged();
            }
        }

        public string BackColor
        {
            get
            {
                string name = FileName?.ToLowerInvariant() ?? "";
                if (name.Contains("{1s}")) return "#001";
                else if (name.Contains("{2e}")) return "#002";
                else if (name.Contains("{3n}")) return "#003";
                else if (name.Contains("{4c}")) return "#004";
                else if (name.Contains("{5j}")) return "#005";
                else if (name.Contains("{6h}")) return "#006";
                return null;
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
