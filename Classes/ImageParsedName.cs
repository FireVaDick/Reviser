using System.IO;
using System.Text.RegularExpressions;

namespace Reviser
{
    public class ImageParsedName
    {
        #region Свойства
        public string OriginalName { get; set; }
        public string Character { get; set; } 
        public string Number { get; set; }     
        public string Author { get; set; }     
        public string Extension { get; set; }
        #endregion



        public static ImageParsedName SetImageParsedName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return null;

            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);

            // 1. Найдём все блоки [автор] и возьмём ПОСЛЕДНИЙ как автора
            var authorMatches = Regex.Matches(nameWithoutExt, @"\[([^\]]+)\]");
            string author = null;
            string tempString = nameWithoutExt;

            if (authorMatches.Count > 0)
            {
                var lastMatch = authorMatches[authorMatches.Count - 1];
                author = lastMatch.Groups[1].Value;
                // Удаляем найденный блок из строки
                tempString = nameWithoutExt.Remove(lastMatch.Index, lastMatch.Length).Trim();
            }

            // 2. В оставшейся строке ищем ПЕРВОЕ (или единственное) число в формате 1, 2.5, 10.3 и т.д.
            var numberMatch = Regex.Match(tempString, @"\b(\d+(?:\.\d+)?)\b");
            if (!numberMatch.Success)
                return null; // Номер обязателен

            string number = numberMatch.Groups[1].Value;
            // Удаляем номер из строки
            string character = tempString.Remove(numberMatch.Index, numberMatch.Length).Trim();

            // 3. Очищаем лишние пробелы в имени персонажа
            character = Regex.Replace(character, @"\s+", " ");

            return new ImageParsedName
			{
                OriginalName = fileName,
                Character = character,
                Number = number,
                Author = author,
                Extension = extension
            };
        }
    }
}
