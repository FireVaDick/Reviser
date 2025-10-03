using System;

namespace Reviser
{
    public class ImageInfoItem
    {
        #region Свойства
        public int Index { get; set; }
        public string FileName { get; set; }
        public string Resolution { get; set; }
        public string AspectRatio { get; set; }
        public string FileSize { get; set; }
        public string Format { get; set; }

        public string BackColor
        {
            get
            {
                string name = FileName?.ToLowerInvariant() ?? "";
                if (name.Contains(" ecchi ")) return "#000";
                if (name.Contains(" porn ")) return "#001";  
                return null;
            }
        }
        public double IndicatorWidth
        {
            get
            {
                double ratio = Convert.ToDouble(AspectRatio);
                double maxWidth = 16; 

                if (ratio >= 1.0) 
                    return maxWidth;
                else return maxWidth * ratio;
            }
        }

        public double IndicatorHeight
        {
            get
            {
                double ratio = Convert.ToDouble(AspectRatio);
                double maxHeight = 16; 

                if (ratio <= 1.0) 
                    return maxHeight;
                else return maxHeight / ratio;
            }
        }
        #endregion
    }
}
