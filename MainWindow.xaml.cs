using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace Reviser
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Шаблоны
        private string nameMoveTemplate = "{character} [author] {number}";
        private string nameFindTemplate = "";
        private string nameReplaceTemplate = "";



        public string NameMoveTemplate
        {
            get => nameMoveTemplate;
            set
            {
                nameMoveTemplate = value;
                OnPropertyChanged();
            }
        }

        public string NameFindTemplate
        {
            get => nameFindTemplate;
            set
            {
                nameFindTemplate = value;
                OnPropertyChanged();
            }
        }

        public string NameReplaceTemplate
        {
            get => nameReplaceTemplate;
            set
            {
                nameReplaceTemplate = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion



        private ObservableCollection<ImageInfoItem> allImageItems = new ObservableCollection<ImageInfoItem>();
        private ObservableCollection<ImageInfoItem> authorImageItems = new ObservableCollection<ImageInfoItem>();
        private ObservableCollection<ImageInfoItem> tagImageItems = new ObservableCollection<ImageInfoItem>();
        private ObservableCollection<ImageNewName> ImageNameMoveFiles { get; set; } = new ObservableCollection<ImageNewName>();
        private ObservableCollection<ImageNewName> ImageNameReplaceFiles { get; set; } = new ObservableCollection<ImageNewName>();

        private CancellationTokenSource cancellationTokenSource;
        private static string directoryPath;

        private Dictionary<string, int> authorStatistics = new Dictionary<string, int>();

        private System.Windows.Threading.DispatcherTimer authorUpdateTimer;
        private bool authorsListNeedsUpdate = false;



        #region Конструктор
        public MainWindow()
        {
            InitializeComponent();

            this.Left = (SystemParameters.FullPrimaryScreenWidth - this.Width) / 2;
            this.Top = (SystemParameters.FullPrimaryScreenHeight - this.Height) / 2;

            AllImageGrid.ItemsSource = allImageItems;
            AuthorImageGrid.ItemsSource = authorImageItems;
            TagImageGrid.ItemsSource = tagImageItems;
            MoveImageGrid.ItemsSource = ImageNameMoveFiles;
            ReplaceImageGrid.ItemsSource = ImageNameReplaceFiles;

            authorUpdateTimer = new System.Windows.Threading.DispatcherTimer();
            authorUpdateTimer.Interval = TimeSpan.FromSeconds(2); // Обновление каждые 2 секунды
            authorUpdateTimer.Tick += AuthorUpdateTimer_Tick;
            authorUpdateTimer.Start();

            DataContext = this;
        }

        private void AuthorUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (authorsListNeedsUpdate && AuthorContainer.Visibility == Visibility.Visible)
            {
                UpdateAuthorsList();
                authorsListNeedsUpdate = false;
            }
        }
        #endregion



        #region Горячие клавиши
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.X:
                    //SwapX_MouseLeftButtonDown(null, null);
                    break;
            }
        }
        #endregion



        #region Переключатели
        private void ShowAllImageGridOption_Checked(object sender, RoutedEventArgs e)
        {
            AllImageGrid.Visibility = Visibility.Visible;
            AuthorImageGrid.Visibility = Visibility.Collapsed;
            TagImageGrid.Visibility = Visibility.Collapsed;
            ReplaceImageGrid.Visibility = Visibility.Collapsed;
            MoveImageGrid.Visibility = Visibility.Collapsed;
        }

        private void ShowAuthorImageGridOption_Checked(object sender, RoutedEventArgs e)
        {
            AllImageGrid.Visibility = Visibility.Hidden;
            AuthorImageGrid.Visibility = Visibility.Visible;
            TagImageGrid.Visibility = Visibility.Collapsed;
            ReplaceImageGrid.Visibility = Visibility.Collapsed;
            MoveImageGrid.Visibility = Visibility.Collapsed;
        }

        private void ShowTagImageGridOption_Checked(object sender, RoutedEventArgs e)
        {
            AllImageGrid.Visibility = Visibility.Collapsed;
            AuthorImageGrid.Visibility = Visibility.Collapsed;
            TagImageGrid.Visibility = Visibility.Visible;
            ReplaceImageGrid.Visibility = Visibility.Collapsed;
            MoveImageGrid.Visibility = Visibility.Collapsed;
        }

        private void ShowReplaceImageGridOption_Checked(object sender, RoutedEventArgs e)
        {
            AllImageGrid.Visibility = Visibility.Visible;
            AuthorImageGrid.Visibility = Visibility.Collapsed;
            TagImageGrid.Visibility = Visibility.Collapsed;
            ReplaceImageGrid.Visibility = Visibility.Visible;
            MoveImageGrid.Visibility = Visibility.Collapsed;
        }

        private void ShowMoveImageGridOption_Checked(object sender, RoutedEventArgs e)
        {
            AllImageGrid.Visibility = Visibility.Collapsed;
            AuthorImageGrid.Visibility = Visibility.Collapsed;
            TagImageGrid.Visibility = Visibility.Collapsed;
            ReplaceImageGrid.Visibility = Visibility.Collapsed;
            MoveImageGrid.Visibility = Visibility.Visible;
        }

        private void ShowAuthorMenuOption_Checked(object sender, RoutedEventArgs e)
        {
            AuthorContainer.Visibility = Visibility.Visible;
            //TagContainer.Visibility = Visibility.Collapsed;
            MoveContainer.Visibility = Visibility.Collapsed;
            ReplaceContainer.Visibility = Visibility.Collapsed;       
            UpdateAuthorsList();
        }

        private void ShowTagMenuOption_Checked(object sender, RoutedEventArgs e)
        {
            AuthorContainer.Visibility = Visibility.Collapsed;
            //TagContainer.Visibility = Visibility.Visible;
            MoveContainer.Visibility = Visibility.Collapsed;
            ReplaceContainer.Visibility = Visibility.Collapsed;           
            UpdateAuthorsList();
        }

        private void ShowReplaceMenuOption_Checked(object sender, RoutedEventArgs e)
        {
            AuthorContainer.Visibility = Visibility.Collapsed;
            //TagContainer.Visibility = Visibility.Collapsed;        
            ReplaceContainer.Visibility = Visibility.Visible;
            MoveContainer.Visibility = Visibility.Collapsed;
        }

        private void ShowMoveMenuOption_Checked(object sender, RoutedEventArgs e)
        {
            AuthorContainer.Visibility = Visibility.Collapsed;
            //TagContainer.Visibility = Visibility.Collapsed;         
            ReplaceContainer.Visibility = Visibility.Collapsed;
            MoveContainer.Visibility = Visibility.Visible;
        }
        #endregion



        #region Главные методы
        private void ChooseImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GetImageDirectory();
        }

        private async void LoadFullTable_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            await LoadTable(true);
        }

        private void CleanEverything_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            allImageItems.Clear();
            authorImageItems.Clear();
            tagImageItems.Clear();
            ImageNameMoveFiles.Clear();
            ImageNameReplaceFiles.Clear();

            AllImageGrid.Items.Refresh();
            AuthorImageGrid.Items.Refresh();
            TagImageGrid.Items.Refresh();
            MoveImageGrid.Items.Refresh();
            ReplaceImageGrid.Items.Refresh();

            authorStatistics.Clear();
            AuthorsWrapPanel.Children.Clear();

            Count.Text = "Всего: 0";
            ClearImagePreview();
        }

        private void Cancel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            cancellationTokenSource?.Cancel();
        }
        #endregion



        #region Реализация главных методов
        private void GetImageDirectory()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            bool? success = fileDialog.ShowDialog();

            if (success == true)
            {
                directoryPath = Path.GetDirectoryName(fileDialog.FileName);
                allImageItems.Clear();
                authorImageItems.Clear();
                authorStatistics.Clear();

                ClearPreviewOnFolderChange();
                SelectedFolder.Text = directoryPath;
            }

            SelectedFolder.Text = directoryPath;
        }

        private async Task LoadTable(bool isFullLoad)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                MessageBox.Show("Сначала выберите папку с изображениями");
                return;
            }

            allImageItems.Clear();
            authorImageItems.Clear();

            // Создаем новый токен отмены
            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Показываем кнопку отмены
            LoadFullTable.IsEnabled = false;
            Cancel.Visibility = Visibility.Visible;

            await Dispatcher.InvokeAsync(() =>
            {
                // Принудительно обновляем привязку данных
                AllImageGrid.Items.Refresh();
                AuthorImageGrid.Items.Refresh();
            });

            try
            {
                await LoadImagesAsync(directoryPath, isFullLoad, false, cancellationToken);

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
                LoadFullTable.IsEnabled = true;
                Cancel.Visibility = Visibility.Collapsed;

                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
        }

        private async Task LoadImagesAsync(string searchFolder, bool isFullLoad, bool isRecursive, CancellationToken cancellationToken)
        {
            string[] filters = new string[] { "jpg", "jpeg", "png", "gif" };
            var filePaths = new List<string>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            foreach (var filter in filters)
            {
                cancellationToken.ThrowIfCancellationRequested();
                filePaths.AddRange(Directory.GetFiles(searchFolder, $"*.{filter}", searchOption));
            }

            allImageItems.Clear();
            authorStatistics.Clear();

            // Обрабатываем файлы с поддержкой отмены
            for (int i = 0; i < filePaths.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var path = filePaths[i];
                var item = await Task.Run(() => SetImageInfo(path, i + 1, isFullLoad, cancellationToken), cancellationToken);

                // Обновляем статистику авторов
                UpdateAuthorStatistics(item);

                // Обновляем UI в основном потоке
                await Dispatcher.InvokeAsync(() =>
                {
                    allImageItems.Add(item);
                    Count.Text = "Всего: " + allImageItems.Count.ToString();
                });

                // Небольшая пауза для плавности с проверкой отмены
                //await Task.Delay(10, cancellationToken);
            }
        }

        private ImageInfoItem SetImageInfo(string fullPath, int index, bool isFullLoad, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var fileInfo = new FileInfo(fullPath);
                string fileName = Path.GetFileName(fullPath);

                // Используем FileStream для чтения изображения без блокировки
                using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var image = System.Drawing.Image.FromStream(fs))
                {
                    int width = image.Width;
                    int height = image.Height;

                    string format = image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg) ? "JPG" :
                                    image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png) ? "PNG" :
                                    image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif) ? "GIF" : "Unknown";
                    
                    var partsOfImageName = PartsOfImageName.ParseImageNameIntoParts(fileName);

                    return new ImageInfoItem
                    {
                        Index = index,
                        FileName = fileName,
                        FilePath = fullPath,
                        Resolution = $"{width}x{height}",
                        AspectRatio = (height / (double)width).ToString("F3"),
                        FileSize = FormatFileSize(fileInfo.Length),
                        Format = format,
                        Character = partsOfImageName.Character,
                        Author = partsOfImageName.Author,
                        Class = partsOfImageName.Class,
                        Number = partsOfImageName.Number,
                        Tags = partsOfImageName.Tags
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
                    FilePath = fullPath,
                    Resolution = "—",
                    AspectRatio = "—",
                    FileSize = "—",
                    Format = "Ошибка"
                };
            }
        }

        private void UpdateAuthorStatistics(ImageInfoItem item)
        {
            if (!string.IsNullOrEmpty(item.Author))
            {
                if (authorStatistics.ContainsKey(item.Author))
                {
                    authorStatistics[item.Author]++;
                }
                else
                {
                    authorStatistics[item.Author] = 1;
                }
            }

            authorsListNeedsUpdate = true;
        }
        #endregion



        #region Обновление генерируемых кнопок статистики                 
        private void UpdateAuthorsList()
        {
            AuthorsWrapPanel.Children.Clear();

            if (authorStatistics.Count == 0)
            {
                NoAuthorText.Visibility = Visibility.Visible;
                return;
            }
            else NoAuthorText.Visibility = Visibility.Collapsed;

            foreach (var author in authorStatistics.OrderByDescending(x => x.Value))
            {
                var authorButton = CreateAuthorButton(author.Key, author.Value);
                AuthorsWrapPanel.Children.Add(authorButton);
            }
        }

        private TextBlock CreateAuthorButton(string authorName, int count)
        {
            var textButton = new TextBlock
            {
                Text = $"{authorName} [{count}]",
                Margin = new Thickness(5),
                Padding = new Thickness(5),
                Background = new SolidColorBrush(Color.FromRgb(221, 223, 233)),
                FontSize = 13,
                Cursor = Cursors.Hand,
                Tag = authorName
            };

            textButton.MouseLeftButtonDown += AuthorButton_MouseLeftButtonDown;
            textButton.MouseEnter += (s, e) =>
            {
                textButton.Background = new SolidColorBrush(Color.FromRgb(174, 194, 235));
            };
            textButton.MouseLeave += (s, e) =>
            {
                textButton.Background = new SolidColorBrush(Color.FromRgb(221, 223, 233));
            };

            return textButton;
        }

        private void AuthorButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textButton && textButton.Tag is string authorName)
            {
                // Фильтруем изображения по автору
                authorImageItems.Clear();
                var filteredItems = allImageItems.Where(item => item.Author == authorName).ToList();

                for (int i = 0; i < filteredItems.Count; i++)
                {
                    filteredItems[i].Index = i + 1;
                    authorImageItems.Add(filteredItems[i]);
                }

                // Показываем фильтрованную таблицу
                ShowAuthorImageGridOption_Checked(null, null);

                if (ShowAuthorImageGridOption != null)
                {
                    ShowAuthorImageGridOption.IsChecked = true;
                }
            }
        }
        #endregion



        #region Дополнительные действия
        private void GetNoAuthor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var itemsToRemove = new List<ImageInfoItem>();

            foreach (var item in allImageItems)
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
                allImageItems.Remove(item);
            }

            // Переиндексируем оставшиеся элементы
            for (int i = 0; i < allImageItems.Count; i++)
            {
                allImageItems[i].Index = i + 1;
            }
        }

        private void CreateFolders_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                MessageBox.Show("Сначала выберите папку с изображениями");
                return;
            }

            if (allImageItems.Count == 0)
            {
                MessageBox.Show("Сначала загрузите таблицу изображений", "Информация",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Создаем подпапки
            string portraitPath = Path.Combine(directoryPath, "Portrait");
            string landscapePath = Path.Combine(directoryPath, "Landscape");
            string squarePath = Path.Combine(directoryPath, "Square");

            Directory.CreateDirectory(portraitPath);
            Directory.CreateDirectory(landscapePath);
            Directory.CreateDirectory(squarePath);

            int movedCount = 0;
            int errorCount = 0;
            int alreadySortedCount = 0;

            foreach (var item in allImageItems)
            {
                try
                {
                    // Проверяем, существует ли файл
                    if (!File.Exists(item.FilePath))
                    {
                        errorCount++;
                        continue;
                    }

                    // Проверяем, не находится ли файл уже в одной из целевых папок
                    string currentDir = Path.GetDirectoryName(item.FilePath);
                    if (currentDir == portraitPath || currentDir == landscapePath || currentDir == squarePath)
                    {
                        alreadySortedCount++;
                        continue;
                    }

                    // Получаем соотношение сторон из уже рассчитанных данных
                    if (double.TryParse(item.AspectRatio.Replace('.', ','), out double aspectRatio))
                    {
                        string destinationFolder;

                        if (aspectRatio < 0.9)
                            destinationFolder = landscapePath;
                        else if (aspectRatio > 1.1)
                            destinationFolder = portraitPath;
                        else
                            destinationFolder = squarePath;

                        // Проверяем, не перемещаем ли мы в ту же папку
                        if (Path.GetDirectoryName(item.FilePath).Equals(destinationFolder, StringComparison.OrdinalIgnoreCase))
                        {
                            alreadySortedCount++;
                            continue;
                        }

                        string fileName = Path.GetFileName(item.FilePath);
                        string destPath = Path.Combine(destinationFolder, fileName);

                        // Если файл уже существует в папке назначения, переименуем
                        if (File.Exists(destPath))
                        {
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                            string extension = Path.GetExtension(fileName);
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
                    else
                    {
                        errorCount++;
                        Debug.WriteLine($"Не удалось распознать соотношение сторон для файла: {item.FileName}");
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Debug.WriteLine($"Не удалось переместить файл {item.FileName}: {ex.Message}");
                }
            }

            // Обновляем отображение таблицы (опционально)
            // Можно перезагрузить таблицу, чтобы увидеть новые пути
            if (MessageBox.Show($"Сортировка завершена!\n\n" +
                               $"Перемещено файлов: {movedCount}\n" +
                               $"Уже отсортировано: {alreadySortedCount}\n" +
                               $"Ошибок: {errorCount}\n\n" +
                               $"Обновить текущую таблицу?",
                               "Информация",
                               MessageBoxButton.YesNo,
                               MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                // Перезагружаем таблицу
                _ = LoadTable(true);
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
                string position = "";

                if (PositionStart.IsChecked == true) 
                    position = "Start";
                else if (PositionEnd.IsChecked == true)
                    position = "End";
                else if (PositionReplace.IsChecked == true)
                    position = "Replace";
                else if (PositionBeforeNumber.IsChecked == true)
                    position = "BeforeNumber";

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



        #region Превью изображений
        [Obsolete]
        private async void AllImageGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedItem = AllImageGrid.SelectedItem as ImageInfoItem;
            if (selectedItem != null && !string.IsNullOrEmpty(directoryPath))
            {
                await LoadImagePreview(selectedItem);
            }
            else
            {
                ClearImagePreview();
            }
        }

        [Obsolete]
        private async void AuthorImageGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedItem = AuthorImageGrid.SelectedItem as ImageInfoItem;
            if (selectedItem != null && !string.IsNullOrEmpty(directoryPath))
            {
                await LoadImagePreview(selectedItem);
            }
            else
            {
                ClearImagePreview();
            }
        }

        [Obsolete]
        private async Task LoadImagePreview(ImageInfoItem item)
        {
            try
            {
                string fullPath = Path.Combine(directoryPath, item.FileName);

                if (File.Exists(fullPath))
                {
                    // Загружаем изображение в фоновом потоке
                    var bitmap = await Task.Run(() => LoadBitmapImage(fullPath));

                    // Устанавливаем в UI потоке
                    await Dispatcher.InvokeAsync(() =>
                    {
                        PreviewImage.Source = bitmap;
                        PreviewFileName.Text = item.FileName;
                        PreviewResolution.Text = $"Разрешение: {item.Resolution}";
                        PreviewFileSize.Text = $"Размер: {item.FileSize}";
                        PreviewFormat.Text = $"Формат: {item.Format}";
                        NoPreviewText.Visibility = Visibility.Collapsed;
                    });
                }
                else
                {
                    ClearImagePreview();
                    PreviewFileName.Text = "Файл не найден";
                }
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    ClearImagePreview();
                    PreviewFileName.Text = $"Ошибка загрузки: {ex.Message}";
                });
            }
        }

        [Obsolete]
        private BitmapImage LoadBitmapImage(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return CreateErrorImage();

                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length == 0)
                    return CreateErrorImage();

                // Используем Uri вместо Stream для избежания проблем с кэшем
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.DecodePixelWidth = 1000;
                bitmap.CreateOptions = BitmapCreateOptions.None;
                bitmap.EndInit();

                // Даем время на загрузку
                if (bitmap.IsDownloading)
                {
                    bitmap.DownloadCompleted += (s, e) =>
                    {
                        bitmap.Freeze();
                    };
                }
                else
                {
                    bitmap.Freeze();
                }

                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки {filePath}: {ex.Message}");
                return CreateErrorImage();
            }
        }

        // Создание изображения-заглушки при ошибке
        [Obsolete]
        private BitmapImage CreateErrorImage()
        {
            try
            {
                // Создаем простое изображение с сообщением об ошибке
                var drawingVisual = new DrawingVisual();
                using (var drawingContext = drawingVisual.RenderOpen())
                {
                    drawingContext.DrawRectangle(System.Windows.Media.Brushes.LightGray, null, new Rect(0, 0, 200, 150));
                    drawingContext.DrawText(
                        new FormattedText("Ошибка загрузки",
                                        System.Globalization.CultureInfo.CurrentCulture,
                                        FlowDirection.LeftToRight,
                                        new Typeface("Arial"),
                                        12,
                                        System.Windows.Media.Brushes.Red),
                        new Point(10, 65));
                }

                var renderTarget = new RenderTargetBitmap(200, 150, 96, 96, PixelFormats.Pbgra32);
                renderTarget.Render(drawingVisual);

                var bitmap = new BitmapImage();
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTarget));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }

                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                // Если даже заглушка не создалась, возвращаем null
                return null;
            }
        }

        private void ClearImagePreview()
        {
            PreviewImage.Source = null;
            PreviewFileName.Text = "";
            PreviewResolution.Text = "";
            PreviewFileSize.Text = "";
            PreviewFormat.Text = "";
            NoPreviewText.Visibility = Visibility.Visible;
        }

        private void ClearPreviewOnFolderChange()
        {
            ClearImagePreview();
            AllImageGrid.SelectedItem = null;
        }

        [Obsolete]
        private void MoveGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedItem = MoveImageGrid.SelectedItem as ImageNewName;
            if (selectedItem != null)
            {
                LoadImagePreviewFromPath(selectedItem.OriginalPath, selectedItem.OriginalName);
            }
            else
            {
                ClearImagePreview();
            }
        }

        [Obsolete]
        private void ReplaceGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedItem = ReplaceImageGrid.SelectedItem as ImageNewName;
            if (selectedItem != null)
            {
                LoadImagePreviewFromPath(selectedItem.OriginalPath, selectedItem.OriginalName);
            }
            else
            {
                ClearImagePreview();
            }
        }

        [Obsolete]
        private async void LoadImagePreviewFromPath(string filePath, string fileName)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var bitmap = await Task.Run(() => LoadBitmapImage(filePath));

                    await Dispatcher.InvokeAsync(() =>
                    {
                        PreviewImage.Source = bitmap;
                        PreviewFileName.Text = fileName;

                        var fileInfo = new FileInfo(filePath);
                        PreviewFileSize.Text = $"Размер: {FormatFileSize(fileInfo.Length)}";

                        // Получаем разрешение из изображения
                        using (var image = System.Drawing.Image.FromFile(filePath))
                        {
                            PreviewResolution.Text = $"Разрешение: {image.Width}x{image.Height}";
                            PreviewFormat.Text = $"Формат: {Path.GetExtension(filePath).ToUpper().TrimStart('.')}";
                        }

                        NoPreviewText.Visibility = Visibility.Collapsed;
                    });
                }
                catch (Exception ex)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ClearImagePreview();
                        PreviewFileName.Text = $"Ошибка загрузки: {ex.Message}";
                    });
                }
            }
            else
            {
                ClearImagePreview();
                PreviewFileName.Text = "Файл не найден";
            }
        }
        #endregion



        #region Обработка двойного клика
        private void AllImageGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var hitTestResult = VisualTreeHelper.HitTest(AllImageGrid, e.GetPosition(AllImageGrid));
            var row = hitTestResult.VisualHit.GetParentOfType<System.Windows.Controls.DataGridRow>();

            OpenImageFromGrid(row, AllImageGrid);
        }

        private void AuthorImageGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var hitTestResult = VisualTreeHelper.HitTest(AuthorImageGrid, e.GetPosition(AuthorImageGrid));
            var row = hitTestResult.VisualHit.GetParentOfType<System.Windows.Controls.DataGridRow>();

            OpenImageFromGrid(row, AuthorImageGrid);
        }

        private void OpenImageFromGrid(System.Windows.Controls.DataGridRow row, System.Windows.Controls.DataGrid grid)
        {
            if (row != null)
            {
                // Выделяем строку, по которой кликнули
                row.IsSelected = true;
            }

            var selectedItem = grid.SelectedItem as ImageInfoItem;
            if (selectedItem != null && !string.IsNullOrEmpty(directoryPath))
            {
                try
                {
                    string fullPath = Path.Combine(directoryPath, selectedItem.FileName);
                    if (File.Exists(fullPath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = fullPath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        MessageBox.Show("Файл не найден", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion



        #region Вспомогательные
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
    }



    public static class VisualTreeHelperExtensions
    {
        public static T GetParentOfType<T>(this DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            if (parent == null)
                return null;

            if (parent is T parentTyped)
                return parentTyped;

            return parent.GetParentOfType<T>();
        }
    }
}
