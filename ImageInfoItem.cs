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

        public string RowColor
        {
            get
            {
                string name = FileName?.ToLowerInvariant() ?? "";
                if (name.Contains(" ecchi ")) return "#000";
                if (name.Contains(" porn ")) return "#001";  
                return null;
            }
        }
        #endregion
    }
}
