﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Reviser
{
    public class ImageNewName : INotifyPropertyChanged
    {
        #region Свойства
        private string newName;

        public int Index { get; set; }
        public string OriginalPath { get; set; }
        public string OriginalName { get; set; }
        public string NewName
        {
            get => newName;
            set
            {
                newName = value;
                OnPropertyChanged();
            }
        }

        public ImageParsedName Parsed { get; set; }

        public string BackColor
        {
            get
            {
                string name = OriginalName?.ToLowerInvariant() ?? "";
                if (name.Contains(" ecchi ")) return "#000";
                if (name.Contains(" porn ")) return "#001";
                return null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion



        public static string SetNewMoveName(ImageParsedName parsedName, string template)
        {
            if (parsedName == null) return null;

            string result = template
                .Replace("{character}", parsedName.Character ?? "")
                .Replace("{number}", parsedName.Number ?? "");

            if (parsedName.Author != null)
            {
                result = result
                    .Replace("[author]", $"[{parsedName.Author}]")
                    .Replace("{author}", parsedName.Author);
            }
            else
            {
                result = result
                    .Replace("[author]", "")
                    .Replace("{author}", "");
            }

            // Убираем двойные/лишние пробелы
            result = Regex.Replace(result, @"\s+", " ").Trim();

            return result + parsedName.Extension;
        }

        public static string SetNewReplaceName(string fileName, string wordToFind, string wordToReplace, string position)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(wordToFind))
                return fileName;

            // Убираем расширение
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);

            // Разбиваем на слова (по пробелам)
            string[] words = nameWithoutExt.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Ищем точное совпадение слова
            int foundIndex = -1;
            for (int i = 0; i < words.Length; i++)
            {
                if (string.Equals(words[i], wordToFind, StringComparison.OrdinalIgnoreCase))
                {
                    foundIndex = i;
                    break;
                }
            }

            // Если не найдено — возвращаем null (исключить из выборки)
            if (foundIndex == -1)
                return null;

            // Удаляем найденное слово
            var newWordList = new List<string>(words);
            newWordList.RemoveAt(foundIndex);

            // Теперь ищем позицию числа
            int numberIndex = -1;
            for (int i = 0; i < newWordList.Count; i++)
            {
                if (Regex.IsMatch(newWordList[i], @"^\d+(?:\.\d+)?$"))
                {
                    numberIndex = i;
                    break; // берём первое число
                }
            }

            // Вставляем замену в указанную позицию
            switch (position)
            {
                case "Start":
                    newWordList.Insert(0, wordToReplace);
                    break;
                case "Middle":
                    newWordList.Insert(numberIndex, wordToReplace);
                    break;
                case "End":
                default:
                    newWordList.Add(wordToReplace);
                    break;
            }

            // Собираем обратно
            string newName = string.Join(" ", newWordList);
            return newName + extension;
        }

    }
}
