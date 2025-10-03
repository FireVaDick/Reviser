using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
namespace Reviser
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Шаблоны
        private string nameMoveTemplate = "{character} [author] {number}";
        private string findTemplate = "";
        private string replaceTemplate = "";



        public string NameMoveTemplate
        {
            get => nameMoveTemplate;
            set
            {
                nameMoveTemplate = value;
                OnPropertyChanged();
            }
        }

        public string FindTemplate
        {
            get => findTemplate;
            set
            {
                findTemplate = value;
                OnPropertyChanged();
            }
        }

        public string ReplaceTemplate
        {
            get => replaceTemplate;
            set
            {
                replaceTemplate = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        private ObservableCollection<ImageNewName> ImageNameMoveFiles { get; set; } = new ObservableCollection<ImageNewName>();
        private ObservableCollection<ImageNewName> ImageNameReplaceFiles { get; set; } = new ObservableCollection<ImageNewName>();

        private readonly ObservableCollection<ImageInfoItem> ImageInfoItems = new ObservableCollection<ImageInfoItem>();
        private static string directoryPath;

        private CancellationTokenSource cancellationTokenSource;



        public MainWindow()
        {
            InitializeComponent();

            this.Left = (SystemParameters.FullPrimaryScreenWidth - this.Width) / 2;
            this.Top = (SystemParameters.FullPrimaryScreenHeight - this.Height) / 2;

            ImageGrid.Items.Clear();
            ImageGrid.ItemsSource = ImageInfoItems;

            ImageNameFilesMoveGrid.Items.Clear();
            ImageNameFilesMoveGrid.ItemsSource = ImageNameMoveFiles;

            ImageNameFilesReplaceGrid.Items.Clear();
            ImageNameFilesReplaceGrid.ItemsSource = ImageNameReplaceFiles;

            DataContext = this;
        }



        #region Загрузка таблицы
        private void ChooseImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            bool? success = fileDialog.ShowDialog();

            if (success == true)
            {
                directoryPath = Path.GetDirectoryName(fileDialog.FileName);
                ImageInfoItems.Clear();
            }

            SelectedFolder.Text = directoryPath;
        }

        private async void LoadTable_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                MessageBox.Show("Сначала выберите папку с изображениями");
                return;
            }

            // Создаем новый токен отмены
            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Показываем кнопку отмены
            Cancel.Visibility = Visibility.Visible;
            LoadTable.IsEnabled = false;

            try
            {
                string[] filters = new string[] { "jpg", "jpeg", "png", "gif" };

                await LoadImagesAsync(directoryPath, filters, false, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    MessageBox.Show("Загрузка отменена");
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}");
            }
            finally
            {
                // Восстанавливаем UI
                Cancel.Visibility = Visibility.Collapsed;
                LoadTable.IsEnabled = true;
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
        }

        private void Cancel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            cancellationTokenSource?.Cancel();
        }

        private async Task LoadImagesAsync(string searchFolder, string[] filters, bool isRecursive, CancellationToken cancellationToken)
        {
            var filePaths = new List<string>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            foreach (var filter in filters)
            {
                cancellationToken.ThrowIfCancellationRequested();
                filePaths.AddRange(Directory.GetFiles(searchFolder, $"*.{filter}", searchOption));
            }

            ImageInfoItems.Clear();

            // Обрабатываем файлы с поддержкой отмены
            for (int i = 0; i < filePaths.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var path = filePaths[i];
                var item = await Task.Run(() => SetImageInfo(path, i + 1, cancellationToken), cancellationToken);

                // Обновляем UI в основном потоке
                await Dispatcher.InvokeAsync(() =>
                {
                    ImageInfoItems.Add(item);
                    Count.Text = "Всего: " + ImageInfoItems.Count.ToString();
                });

                // Небольшая пауза для плавности с проверкой отмены
                await Task.Delay(10, cancellationToken);
            }
        }

        private ImageInfoItem SetImageInfo(string fullPath, int index, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var image = Image.FromFile(fullPath))
                {
                    var fileInfo = new FileInfo(fullPath);
                    string fileName = Path.GetFileName(fullPath);
                    int width = image.Width;
                    int height = image.Height;

                    string format = image.RawFormat.Equals(ImageFormat.Jpeg) ? "JPG" :
                                    image.RawFormat.Equals(ImageFormat.Png) ? "PNG" :
                                    image.RawFormat.Equals(ImageFormat.Gif) ? "GIF" : "Unknown";

                    return new ImageInfoItem
                    {
                        Index = index,
                        FileName = fileName, //currentPath.Replace(directoryPath + "\\", "");
                        Resolution = $"{width}x{height}",
                        AspectRatio = (height / (double)width).ToString("F3"),
                        FileSize = FormatFileSize(fileInfo.Length),
                        Format = format
                    };
                }
            }
            catch (Exception)
            {
                // Логируем ошибку, но не падаем
                return new ImageInfoItem
                {
                    Index = index,
                    FileName = Path.GetFileName(fullPath) + " (ошибка)",
                    Resolution = "—",
                    AspectRatio = "—",
                    FileSize = "—",
                    Format = "Ошибка"
                };
            }
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
        #endregion



        #region Доп фильтры
        private void GetNoAuthor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var itemsToRemove = new List<ImageInfoItem>();

            foreach (var item in ImageInfoItems)
            {
                string fileName = item.FileName;

                // 1. Если есть "Work" — удаляем
                if (fileName.IndexOf("Work", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    itemsToRemove.Add(item);
                    continue;
                }

                // 2. Проверяем наличие автора в квадратных скобках: [...]
                int openBracket = fileName.IndexOf('[');
                int closeBracket = -1;

                if (openBracket > -1)
                    closeBracket = fileName.IndexOf(']', openBracket);

                if (openBracket >= 0 && closeBracket > openBracket)
                {
                    // Найдена конструкция [что-то] → есть автор
                    itemsToRemove.Add(item);
                }
            }

            // Удаляем ненужные элементы из ObservableCollection
            foreach (var item in itemsToRemove)
            {
                ImageInfoItems.Remove(item);
            }

            // Переиндексируем оставшиеся элементы
            for (int i = 0; i < ImageInfoItems.Count; i++)
            {
                ImageInfoItems[i].Index = i + 1;
            }
        }
        #endregion



        #region Подгрузка и сохранение
        private async Task<ObservableCollection<ImageNewName>> LoadImageNameFilesAsync(ObservableCollection<ImageNewName> imageNewNames)
        {
            imageNewNames.Clear();
            if (!Directory.Exists(directoryPath))
                return new ObservableCollection<ImageNewName>();

            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var files = Directory.GetFiles(directoryPath)
                .Where(f => validExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .ToArray();

            // Обрабатываем в фоне
            var parsedItems = await Task.Run(() =>
            {
                var list = new List<ImageNewName>();
                int i = 1;

                foreach (var file in files)
                {
                    var parsed = ImageParsedName.SetImageParsedName(Path.GetFileName(file));
                    if (parsed != null)
                    {
                        list.Add(new ImageNewName
                        {
                            Index = i++,
                            OriginalPath = file,
                            OriginalName = Path.GetFileName(file),
                            Parsed = parsed
                        });
                    }
                }
                return list;
            });

            // Возвращаем как ObservableCollection
            return new ObservableCollection<ImageNewName>(parsedItems);
        }

        private void RenameSave(ObservableCollection<ImageNewName> imageNewNames)
        {
            if (MessageBox.Show("Вы уверены, что хотите переименовать файлы?", "Подтверждение",
               MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            foreach (var item in imageNewNames)
            {
                if (!string.IsNullOrEmpty(item.NewName) && item.NewName != item.OriginalName)
                {
                    try
                    {
                        string newPath = Path.Combine(Path.GetDirectoryName(item.OriginalPath), item.NewName);
                        File.Move(item.OriginalPath, newPath);
                        item.OriginalPath = newPath;
                        item.OriginalName = item.NewName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при переименовании {item.OriginalName}:\n{ex.Message}");
                    }
                }
            }
            MessageBox.Show("Готово!");
        }
        #endregion



        #region Перестановка (Move)
        private async void PreviewMove_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Загружаем и парсим асинхронно
            var items = await LoadImageNameFilesAsync(ImageNameMoveFiles);

            // Применяем шаблон (быстро, можно в UI)
            foreach (var item in items)
            {
                item.NewName = ImageNewName.SetNewMoveName(item.Parsed, NameMoveTemplate);
            }

            // Обновляем UI
            ImageNameMoveFiles.Clear();
            foreach (var item in items)
            {
                ImageNameMoveFiles.Add(item);
            }
        }

        private void RenameMove_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RenameSave(ImageNameMoveFiles);
        }
        #endregion



        #region Замена (Replace)
        private async void PreviewReplace_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var items = await LoadImageNameFilesAsync(ImageNameReplaceFiles);

            string findText = FindTextBox?.Text?.Trim();
            string replaceText = ReplaceTextBox?.Text?.Trim();

            bool useReplaceMode = !string.IsNullOrEmpty(findText) && !string.IsNullOrEmpty(replaceText);

            if (useReplaceMode)
            {
                string position = "End";
                if (PositionStart.IsChecked == true) 
                    position = "Start";
                else if (PositionMiddle.IsChecked == true)
                    position = "Middle";

                // Фильтруем и переименовываем в фоне
                var filteredList = await Task.Run(() =>
                {
                    var result = new List<ImageNewName>();
                    int i = 1;

                    foreach (var item in items)
                    {
                        string newName = ImageNewName.SetNewReplaceName(item.OriginalName, findText, replaceText, position);
                        if (newName != null)
                        {
                            result.Add(new ImageNewName
                            {
                                Index = i++,
                                OriginalPath = item.OriginalPath,
                                OriginalName = item.OriginalName,
                                Parsed = item.Parsed,
                                NewName = newName
                            });
                        }
                    }
                    return result;
                });

                // Обновляем UI
                ImageNameReplaceFiles.Clear();
                foreach (var item in filteredList)
                {
                    ImageNameReplaceFiles.Add(item);
                }
            }
        }

        private void RenameReplace_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RenameSave(ImageNameReplaceFiles);
        }
        #endregion

    }
}
