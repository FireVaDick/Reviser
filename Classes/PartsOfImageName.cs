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

        public List<string> CharacterList { get; set; } = new List<string>();
        public List<string> AuthorList { get; set; } = new List<string>();
        public List<string> TagList { get; set; } = new List<string>();



        static public PartsOfImageName ParseImageNameIntoParts(string fileName)
        {
            var result = new PartsOfImageName();
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

            result.Author = ExtractAndRemove(ref nameWithoutExt, @"\[([^\]]+)\]");

            if (!string.IsNullOrEmpty(result.Author))
            {
                result.AuthorList = result.Author.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(author => author.Trim())
                                                 .Where(author => !string.IsNullOrEmpty(author))
                                                 .ToList();
            }

            result.Class = ExtractAndRemove(ref nameWithoutExt, @"\{([^\}]+)\}");

            result.Tags = ExtractAndRemove(ref nameWithoutExt, @"@(.+)$");

            if (!string.IsNullOrEmpty(result.Tags))
            {
                result.TagList = result.Tags.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(tag => tag.Trim())
                                            .Where(tag => !string.IsNullOrEmpty(tag))
                                            .ToList();
            }

            result.Number = ExtractAndRemove(ref nameWithoutExt, @"\s+(\d+(?:\.\d+)?)$");

            // Если не нашли число с пробелом, пробуем без пробела
            if (string.IsNullOrEmpty(result.Number))
            {
                result.Number = ExtractAndRemove(ref nameWithoutExt, @"(\d+(?:\.\d+)?)$");
            }

            result.Character = nameWithoutExt.Trim();

            if (!string.IsNullOrEmpty(result.Character))
            {
                result.CharacterList = result.Character.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                                                       .Select(character => character.Trim())
                                                       .Where(character => !string.IsNullOrEmpty(character))
                                                       .ToList();
            }

            return result;
        }

        private static string ExtractAndRemove(ref string text, string pattern)
        {
            var match = Regex.Match(text, pattern);
            if (match.Success)
            {
                string value = match.Groups[1].Value;
                text = text.Remove(match.Index, match.Length).Trim();
                return value;
            }
            return null;
        }
    }
}
