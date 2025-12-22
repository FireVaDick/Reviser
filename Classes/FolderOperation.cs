using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reviser.Classes
{
    class FolderOperation
    {
        public static (int movedCount, int errorCount, int alreadySortedCount) MoveFilesToFolders(
                       IEnumerable<ImageInfoItem> items,
                       string baseDirectory,
                       Func<ImageInfoItem, int, string> getFolderNameFunc,
                       Func<ImageInfoItem, string, string> getFileNameFunc = null)
        {
            int movedCount = 0;
            int errorCount = 0;
            int alreadySortedCount = 0;

            foreach (var item in items)
            {
                try
                {
                    if (!File.Exists(item.FilePath))
                    {
                        errorCount++;
                        continue;
                    }

                    string fileName = Path.GetFileName(item.FilePath);
                    string currentDir = Path.GetDirectoryName(item.FilePath);
                    string destinationFolder = getFolderNameFunc(item, items.ToList().IndexOf(item));

                    // Создаем папку, если её нет
                    Directory.CreateDirectory(destinationFolder);

                    // Проверяем, не находится ли файл уже в целевой папке
                    if (currentDir.Equals(destinationFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        alreadySortedCount++;
                        continue;
                    }

                    // Получаем имя файла (может быть изменено)
                    string newFileName = getFileNameFunc != null
                        ? getFileNameFunc(item, fileName)
                        : fileName;

                    string destPath = Path.Combine(destinationFolder, newFileName);

                    // Если файл уже существует в папке назначения, переименуем
                    if (File.Exists(destPath))
                    {
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(newFileName);
                        string extension = Path.GetExtension(newFileName);
                        int counter = 1;
                        do
                        {
                            destPath = Path.Combine(destinationFolder, $"{nameWithoutExt}_{counter}{extension}");
                            counter++;
                        } while (File.Exists(destPath));
                    }

                    // Перемещаем файл
                    File.Move(item.FilePath, destPath);

                    // Обновляем путь в элементе таблицы
                    item.FilePath = destPath;
                    movedCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Debug.WriteLine($"Не удалось переместить файл {item.FileName}: {ex.Message}");
                }
            }

            return (movedCount, errorCount, alreadySortedCount);
        }
    }
}
