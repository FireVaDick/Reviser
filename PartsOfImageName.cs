using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Reviser
{
    class PartsOfImageName
    {
        public string Character { get; set; }
        public string Author { get; set; }
        public string Class { get; set; }
        public string Number { get; set; }
        public string Tags { get; set; }



        static public PartsOfImageName ParseImageNameIntoParts(string fileName)
        {
            var result = new PartsOfImageName();
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

            var authorMatch = Regex.Match(nameWithoutExt, @"\[([^\]]+)\]");
            if (authorMatch.Success)
            {
                result.Author = authorMatch.Groups[1].Value;
                nameWithoutExt = nameWithoutExt.Remove(authorMatch.Index, authorMatch.Length).Trim();
            }

            var classMatch = Regex.Match(nameWithoutExt, @"\{([^\}]+)\}");
            if (classMatch.Success)
            {
                result.Class = classMatch.Groups[1].Value;
                nameWithoutExt = nameWithoutExt.Remove(classMatch.Index, classMatch.Length).Trim();
            }

            var numberMatch = Regex.Match(nameWithoutExt, @"(\d+(?:\.\d+)?)$");
            if (numberMatch.Success)
            {
                result.Number = numberMatch.Groups[1].Value;
                nameWithoutExt = nameWithoutExt.Remove(numberMatch.Index).Trim();
            }

            var tagsMatch = Regex.Match(nameWithoutExt, @"@(.+)$");
            if (tagsMatch.Success)
            {
                result.Tags = tagsMatch.Groups[1].Value;
                nameWithoutExt = nameWithoutExt.Remove(tagsMatch.Index).Trim();
            }

            result.Character = nameWithoutExt.Trim();

            return result;
        }
    }
}
