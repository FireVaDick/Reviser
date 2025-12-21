using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Point = System.Windows.Point;

namespace Reviser
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Шаблоны
        private string moveTemplate = "{character} [author] {number}";
        private string findTemplate = "";
        private string replaceTemplate = "";

        private int characterAmountTemplate = 3;
        private int authorAmountTemplate = 3;
        private int tagAmountTemplate = 6;

        private bool characterListNeedsUpdate = false;
        private bool authorListNeedsUpdate = false;
        private bool tagListNeedsUpdate = false;

        private DispatcherTimer characterUpdateTimer;
        private DispatcherTimer authorUpdateTimer;
        private DispatcherTimer tagUpdateTimer;


        public string MoveTemplate
        {
            get => moveTemplate;
            set
            {
                moveTemplate = value;
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

        public int CharacterAmountTemplate
        {
            get => characterAmountTemplate;
            set
            {
                characterAmountTemplate = value;
                OnPropertyChanged();
                characterListNeedsUpdate = true;
            }
        }

        public int AuthorAmountTemplate
        {
            get => authorAmountTemplate;
            set
            {
                authorAmountTemplate = value;
                OnPropertyChanged();
                authorListNeedsUpdate = true;
            }
        }

        public int TagAmountTemplate
        {
            get => tagAmountTemplate;
            set
            {
                tagAmountTemplate = value;
                OnPropertyChanged();
                tagListNeedsUpdate = true;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        private ObservableCollection<ImageInfoItem> allImageItems = new ObservableCollection<ImageInfoItem>();
        private ObservableCollection<ImageInfoItem> characterImageItems = new ObservableCollection<ImageInfoItem>();
        private ObservableCollection<ImageInfoItem> authorImageItems = new ObservableCollection<ImageInfoItem>();
        private ObservableCollection<ImageInfoItem> tagImageItems = new ObservableCollection<ImageInfoItem>();
        private ObservableCollection<ImageNewName> moveImageFiles = new ObservableCollection<ImageNewName>();
        private ObservableCollection<ImageNewName> replaceImageFiles = new ObservableCollection<ImageNewName>();

        private Dictionary<string, int> characterStatistics = new Dictionary<string, int>();
        private Dictionary<string, int> authorStatistics = new Dictionary<string, int>();
        private Dictionary<string, int> tagStatistics = new Dictionary<string, int>();

        private CancellationTokenSource cancellationTokenSource;
        private static string directoryPath;



        #region Конструктор
        public MainWindow()
        {
            InitializeComponent();

            this.Left = (SystemParameters.FullPrimaryScreenWidth - this.Width) / 2;
            this.Top = (SystemParameters.FullPrimaryScreenHeight - this.Height) / 2;

            AllDataGrid.ItemsSource = allImageItems;
            CharacterDataGrid.ItemsSource = characterImageItems;
            AuthorDataGrid.ItemsSource = authorImageItems;
            TagDataGrid.ItemsSource = tagImageItems;
            MoveDataGrid.ItemsSource = moveImageFiles;
            ReplaceDataGrid.ItemsSource = replaceImageFiles;

            SetTimers(2);

            this.DataContext = this;
        }
        #endregion



        #region Таймеры
        private void SetTimers(int seconds)
        {
            characterUpdateTimer = new System.Windows.Threading.DispatcherTimer();
            characterUpdateTimer.Interval = TimeSpan.FromSeconds(seconds);
            characterUpdateTimer.Tick += CharacterUpdateTimer_Tick;
            characterUpdateTimer.Start();

            authorUpdateTimer = new System.Windows.Threading.DispatcherTimer();
            authorUpdateTimer.Interval = TimeSpan.FromSeconds(seconds); 
            authorUpdateTimer.Tick += AuthorUpdateTimer_Tick;
            authorUpdateTimer.Start();

            tagUpdateTimer = new System.Windows.Threading.DispatcherTimer();
            tagUpdateTimer.Interval = TimeSpan.FromSeconds(seconds); 
            tagUpdateTimer.Tick += TagUpdateTimer_Tick;
            tagUpdateTimer.Start();
        }

        private void CharacterUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (characterListNeedsUpdate && CharacterActionGrid.Visibility == Visibility.Visible)
            {
                UpdateCharacterList();
                characterListNeedsUpdate = false;
            }
        }

        private void AuthorUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (authorListNeedsUpdate && AuthorActionGrid.Visibility == Visibility.Visible)
            {
                UpdateAuthorList();
                authorListNeedsUpdate = false;
            }
        }

        private void TagUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (tagListNeedsUpdate && TagActionGrid.Visibility == Visibility.Visible)
            {
                UpdateTagList();
                tagListNeedsUpdate = false;
            }
        }
        #endregion



        #region Фильтры ComboBox
        private void CharacterFilterTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCharacterList();
        }

        private void AuthorFilterTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAuthorList();
        }

        private void TagFilterTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTagList();
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



        #region Переключатели ClassGroup
        private void Class1s_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Class2e_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Class3n_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Class4c_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Class5j_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Class6h_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        #endregion



        #region Переключатели DataGroup
        private void ShowCurrentDataGrid(DataGrid dataGridToShow)
        {
            var allGrids = new List<DataGrid>
            {
                AllDataGrid,
                CharacterDataGrid,
                AuthorDataGrid,
                TagDataGrid,
                ReplaceDataGrid,
                MoveDataGrid
            };

            foreach (var grid in allGrids)
            {
                grid.Visibility = Visibility.Collapsed;
            }

            if (dataGridToShow != null)
            {
                dataGridToShow.Visibility = Visibility.Visible;
            }
        }

        private void ShowAllDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowCurrentDataGrid(AllDataGrid);
        }

        private void ShowCharacterDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowCurrentDataGrid(CharacterDataGrid);
        }

        private void ShowAuthorDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowCurrentDataGrid(AuthorDataGrid);
        }

        private void ShowTagDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowCurrentDataGrid(TagDataGrid);
        }

        private void ShowReplaceDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowCurrentDataGrid(ReplaceDataGrid);
        }

        private void ShowMoveDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowCurrentDataGrid(MoveDataGrid);
        }
        #endregion



        #region Переключатели ActionGroup
        private void ShowCurrentActionGrid(Grid actionGridToShow)
        {
            var allGrids = new List<Grid>
            {
                CharacterActionGrid,
                AuthorActionGrid,
                TagActionGrid,
                RenameActionGrid,
                OtherActionGrid
            };

            foreach (var grid in allGrids)
            {
                grid.Visibility = Visibility.Collapsed;
            }

            if (actionGridToShow != null)
            {
                actionGridToShow.Visibility = Visibility.Visible;
            }
        }

        private void ShowCharacterActionGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowCurrentActionGrid(CharacterActionGrid);
            UpdateCharacterList();
        }

        private void ShowAuthorActionGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowCurrentActionGrid(AuthorActionGrid);
            UpdateAuthorList();
        }

        private void ShowTagActionGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowCurrentActionGrid(TagActionGrid);
            UpdateTagList();
        }

        private void ShowRenameActionGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowCurrentActionGrid(RenameActionGrid);
        }

        private void ShowOtherActionGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowCurrentActionGrid(OtherActionGrid);
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
            Clean();
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
                Clean();

                directoryPath = Path.GetDirectoryName(fileDialog.FileName);          
                SelectedFolder.Text = directoryPath;
            }
        }

        private async Task LoadTable(bool isFullLoad)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                MessageBox.Show("Сначала выберите папку с изображениями");
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            LoadFullTable.IsEnabled = false;
            Cancel.Visibility = Visibility.Visible;

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
                LoadFullTable.IsEnabled = true;
                Cancel.Visibility = Visibility.Collapsed;

                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
        }

        private async Task LoadImagesAsync(string searchFolder, bool isFullLoad, bool isRecursive, CancellationToken cancellationToken)
        {
            string[] filters = new string[] { "jpg", "jpeg", "png", "gif" };
            var filePaths = GetFilePaths(searchFolder, filters, isRecursive);

            for (int i = 0; i < filePaths.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var path = filePaths[i];
                var item = await Task.Run(() => SetImageInfo(path, i + 1, isFullLoad, cancellationToken), cancellationToken);

                UpdateCharacterStatistics(item);
                UpdateAuthorStatistics(item);
                UpdateTagStatistics(item);

                await UpdateUIWithItem(item);
            }
        }

        private List<string> GetFilePaths(string folder, string[] filters, bool recursive)
        {
            var filePaths = new List<string>();
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            foreach (var filter in filters)
            {
                filePaths.AddRange(Directory.GetFiles(folder, $"*.{filter}", searchOption));
            }

            return filePaths;
        }

        private async Task UpdateUIWithItem(ImageInfoItem item)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                allImageItems.Add(item);
                Count.Text = "Всего: " + allImageItems.Count.ToString();
            });
        }

        private ImageInfoItem SetImageInfo(string fullPath, int index, bool isFullLoad, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var fileInfo = new FileInfo(fullPath);
                string fileName = Path.GetFileName(fullPath);

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
                        Tags = partsOfImageName.Tags,
                        CharacterList = partsOfImageName.CharacterList,
                        AuthorList = partsOfImageName.AuthorList,
                        TagList = partsOfImageName.TagList
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

        private void Clean()
        {
            allImageItems.Clear();
            characterImageItems.Clear();
            authorImageItems.Clear();
            tagImageItems.Clear();
            moveImageFiles.Clear();
            replaceImageFiles.Clear();

            AllDataGrid.Items.Refresh();
            CharacterDataGrid.Items.Refresh();
            AuthorDataGrid.Items.Refresh();
            TagDataGrid.Items.Refresh();
            MoveDataGrid.Items.Refresh();
            ReplaceDataGrid.Items.Refresh();

            characterStatistics.Clear();
            authorStatistics.Clear();
            tagStatistics.Clear();

            CharacterWrapPanel.Children.Clear();
            AuthorWrapPanel.Children.Clear();
            TagWrapPanel.Children.Clear();

            Count.Text = "Всего: 0";
            ClearImagePreview();
        }
        #endregion



        #region Общие методы для работы со статистикой
        private void UpdateList(Dictionary<string, int> statistics,
                               WrapPanel wrapPanel,
                               TextBlock noTextElement,
                               ComboBox filterComboBox,
                               int amountTemplate,
                               Action<object, MouseButtonEventArgs> buttonClickHandler)
        {
            wrapPanel.Children.Clear();

            string filterType = GetComboBoxFilterType(filterComboBox);
            List<KeyValuePair<string, int>> filteredItems;

            if (filterType == ">=")
            {
                filteredItems = statistics
                    .Where(x => x.Value >= amountTemplate)
                    .OrderByDescending(x => x.Value)
                    .ToList();
            }
            else
            {
                filteredItems = statistics
                    .Where(x => x.Value <= amountTemplate)
                    .OrderByDescending(x => x.Value)
                    .ToList();
            }

            if (!filteredItems.Any())
            {
                noTextElement.Visibility = Visibility.Visible;
                return;
            }

            noTextElement.Visibility = Visibility.Collapsed;

            foreach (var item in filteredItems)
            {
                var button = new TextBlock
                {
                    Text = $"{item.Key} [{item.Value}]",
                    Margin = new Thickness(5),
                    Padding = new Thickness(5),
                    Background = new SolidColorBrush(Color.FromRgb(221, 223, 233)),
                    FontSize = 13,
                    Cursor = Cursors.Hand,
                    Tag = item.Key
                };

                button.MouseLeftButtonDown += (s, e) => buttonClickHandler(s, e);
                button.MouseEnter += (s, e) =>
                {
                    button.Background = new SolidColorBrush(Color.FromRgb(174, 194, 235));
                };
                button.MouseLeave += (s, e) =>
                {
                    button.Background = new SolidColorBrush(Color.FromRgb(221, 223, 233));
                };

                wrapPanel.Children.Add(button);
            }
        }

        private void UpdateStatistics(ImageInfoItem item,
                                      Dictionary<string, int> statistics,
                                      List<string> list,
                                      ref bool listNeedsUpdate)
        {
            if (list != null && list.Any())
            {
                foreach (var element in list)
                {
                    if (!string.IsNullOrEmpty(element))
                    {
                        if (statistics.ContainsKey(element))
                        {
                            statistics[element]++;
                        }
                        else
                        {
                            statistics[element] = 1;
                        }
                    }
                }
            }

            listNeedsUpdate = true;
        }

        private void FilterAndShowItems(string filterName,
                               ObservableCollection<ImageInfoItem> targetCollection,
                               Func<ImageInfoItem, bool> filterPredicate,
                               OneMenuOption showDataGridControl,
                               DataGrid targetDataGrid)
        {
            targetCollection.Clear();
            var filteredItems = allImageItems.Where(filterPredicate).ToList();

            for (int i = 0; i < filteredItems.Count; i++)
            {
                filteredItems[i].Index = i + 1;
                targetCollection.Add(filteredItems[i]);
            }

            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = showDataGridControl
            };

            showDataGridControl.RaiseEvent(args);
            ShowCurrentDataGrid(targetDataGrid);
        }
        #endregion



        #region Cтатистика персонажей
        private void UpdateCharacterStatistics(ImageInfoItem item)
        {
            UpdateStatistics(item, characterStatistics, item.CharacterList, ref characterListNeedsUpdate);
        }

        private void UpdateCharacterList()
        {
            UpdateList(characterStatistics, CharacterWrapPanel, NoCharacterText,
                CharacterFilterTypeComboBox, CharacterAmountTemplate, CharacterButton_MouseLeftButtonDown);
        }

        private void CharacterButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textButton && textButton.Tag is string characterName)
            {
                FilterAndShowItems(characterName, characterImageItems,
                    i => i.CharacterList?.Contains(characterName) == true,
                    ShowCharacterDataGrid, CharacterDataGrid);
            }
        }
        #endregion



        #region Cтатистика авторов
        private void UpdateAuthorStatistics(ImageInfoItem item)
        {
            UpdateStatistics(item, authorStatistics, item.AuthorList, ref authorListNeedsUpdate);
        }

        private void UpdateAuthorList()
        {
            UpdateList(authorStatistics, AuthorWrapPanel, NoAuthorText, 
                AuthorFilterTypeComboBox, AuthorAmountTemplate, AuthorButton_MouseLeftButtonDown);
        }

        private void AuthorButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textButton && textButton.Tag is string authorName)
            {
                FilterAndShowItems(authorName, authorImageItems,
                    i => i.AuthorList?.Contains(authorName) == true,
                    ShowAuthorDataGrid, AuthorDataGrid);
            }
        }
        #endregion



        #region Статистика тегов
        private void UpdateTagStatistics(ImageInfoItem item)
        {
            UpdateStatistics(item, tagStatistics, item.TagList, ref tagListNeedsUpdate);
        }

        private void UpdateTagList()
        {
            UpdateList(tagStatistics, TagWrapPanel, NoTagText, 
                TagFilterTypeComboBox, TagAmountTemplate, TagButton_MouseLeftButtonDown);
        }

        private void TagButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textButton && textButton.Tag is string tagName)
            {
                FilterAndShowItems(tagName, tagImageItems, 
                    i => i.TagList?.Contains(tagName) == true, 
                    ShowTagDataGrid, TagDataGrid);
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

                        if (aspectRatio < 0.85)
                            destinationFolder = landscapePath;
                        else if (aspectRatio > 1.15)
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
            var items = await LoadImageNameFilesAsync(moveImageFiles);

            // Применяем шаблон (быстро, можно в UI)
            foreach (var item in items)
            {
                item.NewName = ImageNewName.SetNewMoveName(item.Parsed, MoveTemplate);
            }

            // Обновляем UI
            moveImageFiles.Clear();
            foreach (var item in items)
            {
                moveImageFiles.Add(item);
            }

            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = ShowMoveDataGrid
            };

            ShowMoveDataGrid.RaiseEvent(args);
            ShowCurrentDataGrid(MoveDataGrid);
        }

        private void RenameMove_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RenameSave(moveImageFiles);
        }
        #endregion



        #region Замена (Replace)
        private async void PreviewReplace_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var items = await LoadImageNameFilesAsync(replaceImageFiles);

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
                replaceImageFiles.Clear();
                foreach (var item in filteredList)
                {
                    replaceImageFiles.Add(item);
                }

                var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
                {
                    RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                    Source = ShowReplaceDataGrid
                };

                ShowReplaceDataGrid.RaiseEvent(args);
                ShowCurrentDataGrid(ReplaceDataGrid);
            }
        }

        private void RenameReplace_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RenameSave(replaceImageFiles);
        }
        #endregion



        #region Обработка превью (SelectionChanged)
        private void AllDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadImageWithDataGrid(AllDataGrid);
        }

        private void CharacterDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadImageWithDataGrid(CharacterDataGrid);
        }

        private void AuthorDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadImageWithDataGrid(AuthorDataGrid);
        }

        private void TagDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadImageWithDataGrid(TagDataGrid);
        }

        private async void LoadImageWithDataGrid(DataGrid grid)
        {
            var selectedItem = grid.SelectedItem as ImageInfoItem;

            if (selectedItem != null && !string.IsNullOrEmpty(directoryPath))
            {
                await LoadImagePreviewAsync(selectedItem);
            }
            else
            {
                ClearImagePreview();
            }
        }

        private async Task LoadImagePreviewAsync(ImageInfoItem item)
        {
            try
            {
                string fullPath = Path.Combine(directoryPath, item.FileName);

                if (File.Exists(fullPath))
                {
                    var bitmap = await LoadBitmapImageAsync(fullPath);

                    await Dispatcher.InvokeAsync(() =>
                    {
                        PreviewImage.Source = bitmap;
                        PreviewFileName.Text = item.FileName;

                        PreviewCharacter.Text = $"Персонаж: {item.Character}";
                        PreviewAuthor.Text = $"Автор: {item.Author}";
                        PreviewTags.Text = $"Теги: {item.Tags}";

                        PreviewResolution.Text = $"Разрешение: {item.Resolution}";
                        PreviewFileSize.Text = $"Размер: {item.FileSize}";

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

        private async Task<BitmapImage> LoadBitmapImageAsync(string filePath)
        {
            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
            bitmap.DecodePixelWidth = 1000;
            bitmap.EndInit();

            // Если изображение еще загружается, ждем завершения
            if (bitmap.IsDownloading)
            {
                await Task.Run(() =>
                {
                    // Ждем завершения загрузки
                    var waitEvent = new ManualResetEventSlim(false);
                    bitmap.DownloadCompleted += (s, e) => waitEvent.Set();
                    waitEvent.Wait();
                });
            }

            bitmap.Freeze(); 
            return bitmap;
        }

        private void ClearImagePreview()
        {
            PreviewImage.Source = null;
            PreviewFileName.Text = "";

            PreviewCharacter.Text = "";
            PreviewAuthor.Text = "";
            PreviewTags.Text = "";

            PreviewResolution.Text = "";
            PreviewFileSize.Text = "";

            NoPreviewText.Visibility = Visibility.Visible;
        }
        #endregion



        #region Обработка открытия (MouseDoubleClick)
        private void AllDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenImageFromDataGrid(AllDataGrid, e);
        }

        private void CharacterDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenImageFromDataGrid(CharacterDataGrid, e);
        }

        private void AuthorDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenImageFromDataGrid(AuthorDataGrid, e);
        }

        private void TagDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenImageFromDataGrid(TagDataGrid, e);
        }

        private void OpenImageFromDataGrid(DataGrid grid, MouseButtonEventArgs e)
        {
            var hitTestResult = VisualTreeHelper.HitTest(grid, e.GetPosition(grid));
            var row = hitTestResult.VisualHit.GetParentOfType<DataGridRow>();

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

        private string GetComboBoxFilterType(ComboBox comboBox)
        {
            if (comboBox == null) return ">=";

            if (comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content?.ToString() ?? ">=";
            }

            return ">=";
        }
        #endregion
    }



    #region Другое
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
    #endregion
}
