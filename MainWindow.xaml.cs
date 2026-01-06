using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
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
using System.Windows.Threading;

namespace Reviser
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Шаблоны
        private string moveTemplate = "{character} [author] {number}";
        private string findTemplate = "";
        private string replaceTemplate = "";

        private double ratioEdgeTemplate = 0.15;
        private int maxAmountTemplate = 100;

        private int characterAmountTemplate = 2;
        private int authorAmountTemplate = 2;
        private int tagAmountTemplate = 5;

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

        public double RatioEdgeTemplate
        {
            get => ratioEdgeTemplate;
            set
            {
                ratioEdgeTemplate = value;
                OnPropertyChanged();
            }
        }

        public int MaxAmountTemplate
        {
            get => maxAmountTemplate;
            set
            {
                maxAmountTemplate = value;
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

        private HashSet<string> includeCharacters = new HashSet<string>();
        private HashSet<string> excludeCharacters = new HashSet<string>();
        private HashSet<string> includeAuthors = new HashSet<string>();
        private HashSet<string> excludeAuthors = new HashSet<string>();
        private HashSet<string> includeTags = new HashSet<string>();
        private HashSet<string> excludeTags = new HashSet<string>();

        private ObservableCollection<ImageInfoItem> sourceAllImageItems = new ObservableCollection<ImageInfoItem>();
        private ObservableCollection<ImageInfoItem> sourceCharacterImageItems = new ObservableCollection<ImageInfoItem>();
        private ObservableCollection<ImageInfoItem> sourceAuthorImageItems = new ObservableCollection<ImageInfoItem>();
        private ObservableCollection<ImageInfoItem> sourceTagImageItems = new ObservableCollection<ImageInfoItem>();
              
        private ObservableCollection<ImageInfoItem> classedAllImageItems = new ObservableCollection<ImageInfoItem>();
        private ObservableCollection<ImageInfoItem> classedCharacterImageItems = new ObservableCollection<ImageInfoItem>();
        private ObservableCollection<ImageInfoItem> classedAuthorImageItems = new ObservableCollection<ImageInfoItem>();
        private ObservableCollection<ImageInfoItem> classedTagImageItems = new ObservableCollection<ImageInfoItem>();
              
        private ObservableCollection<ImageNewName> moveImageFiles = new ObservableCollection<ImageNewName>();
        private ObservableCollection<ImageNewName> replaceImageFiles = new ObservableCollection<ImageNewName>();

        private Dictionary<string, int> characterStatistics = new Dictionary<string, int>();
        private Dictionary<string, int> authorStatistics = new Dictionary<string, int>();
        private Dictionary<string, int> tagStatistics = new Dictionary<string, int>();

        private CancellationTokenSource cancellationTokenSource;
        private static string firstFilePath;
        private static string directoryPath;

        private List<string> enabledClasses = new List<string> { "1s", "2e", "3n", "4c", "5j", "6h" };
        private DataGrid currentDataGrid;



        #region Конструктор
        public MainWindow()
        {
            InitializeComponent();

            this.Left = (SystemParameters.FullPrimaryScreenWidth - this.Width) / 2;
            this.Top = (SystemParameters.FullPrimaryScreenHeight - this.Height) / 2;

            AllDataGrid.ItemsSource = classedAllImageItems;
            CharacterDataGrid.ItemsSource = classedCharacterImageItems;
            AuthorDataGrid.ItemsSource = classedAuthorImageItems;
            TagDataGrid.ItemsSource = classedTagImageItems;

            MoveDataGrid.ItemsSource = moveImageFiles;
            ReplaceDataGrid.ItemsSource = replaceImageFiles;

            currentDataGrid = AllDataGrid;

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



        #region Горячие клавиши
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F2:
                    StartEditingSelectedRow();
                    e.Handled = true;
                    break;
                case Key.Enter:
                    CommitEditAndRename();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    CancelEdit();
                    e.Handled = true;
                    break;
            }
        }
        #endregion



        #region Переименовать
        private TextBox editingTextBox;
        private ImageInfoItem editingItem;
        private string originalEditingFileName;

        private void StartEditingSelectedRow()
        {
            if (currentDataGrid == null || currentDataGrid.SelectedItem == null)
                return;

            // Проверяем, какая таблица активна и можно ли редактировать
            if (currentDataGrid == AllDataGrid ||
                currentDataGrid == CharacterDataGrid ||
                currentDataGrid == AuthorDataGrid ||
                currentDataGrid == TagDataGrid)
            {
                if (currentDataGrid.SelectedItem is ImageInfoItem selectedItem)
                {
                    StartEditingImageInfoItem(selectedItem);
                }
            }
        }

        private void StartEditingImageInfoItem(ImageInfoItem item)
        {
            editingItem = item;
            originalEditingFileName = item.FileName;

            // Находим строку в DataGrid
            var row = currentDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
            if (row == null) return;

            // Находим колонку с именем файла (обычно вторая колонка с Header="Имя")
            DataGridColumn nameColumn = null;
            foreach (var column in currentDataGrid.Columns)
            {
                if (column is DataGridTextColumn textColumn && textColumn.Header?.ToString() == "Имя")
                {
                    nameColumn = column;
                    break;
                }
            }

            if (nameColumn == null) return;

            // Получаем ячейку
            var cellContent = nameColumn.GetCellContent(row);
            if (cellContent is TextBlock textBlock)
            {
                // Создаем TextBox для редактирования
                editingTextBox = new TextBox
                {
                    Text = item.FileName,
                    FontSize = 16,
                    FontFamily = new FontFamily("pack://application:,,,/Fonts/#v_DistrictTF-Regular"),
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(2),
                    VerticalContentAlignment = VerticalAlignment.Center
                };

                // Заменяем TextBlock на TextBox
                var parent = textBlock.Parent as FrameworkElement;
                if (parent != null)
                {
                    // Сохраняем оригинальный контент
                    parent.Tag = textBlock; // сохраняем для восстановления

                    // Добавляем TextBox
                    if (parent is Border border)
                    {
                        border.Child = editingTextBox;
                    }
                    else if (parent is ContentPresenter presenter)
                    {
                        presenter.Content = editingTextBox;
                    }
                    else
                    {
                        // Пытаемся найти ContentControl
                        var contentControl = FindVisualParent<ContentControl>(textBlock);
                        if (contentControl != null)
                        {
                            contentControl.Content = editingTextBox;
                        }
                    }

                    // Фокусируемся на TextBox
                    editingTextBox.Focus();
                    editingTextBox.SelectAll();

                    // Обработчики событий
                    editingTextBox.KeyDown += EditingTextBox_KeyDown;
                    editingTextBox.LostFocus += EditingTextBox_LostFocus;
                }
            }
        }

        private void EditingTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitEditAndRename();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CancelEdit();
                e.Handled = true;
            }
        }

        private void EditingTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Если потеряли фокус и не было сохранения, отменяем
            if (editingTextBox != null && !editingTextBox.IsKeyboardFocusWithin)
            {
                CancelEdit();
            }
        }

        private void CommitEditAndRename()
        {
            if (editingItem == null || editingTextBox == null) return;

            string newFileName = editingTextBox.Text.Trim();

            if (string.IsNullOrEmpty(newFileName) || newFileName == originalEditingFileName)
            {
                CancelEdit();
                return;
            }

            try
            {
                // Полный путь к файлу
                string originalFullPath = editingItem.FilePath;
                string directory = Path.GetDirectoryName(originalFullPath);
                string extension = Path.GetExtension(originalFullPath);

                // Проверяем, что новое имя имеет правильное расширение
                if (!newFileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    newFileName += extension;
                }

                string newFullPath = Path.Combine(directory, newFileName);

                // Проверяем, не существует ли уже файл с таким именем
                if (File.Exists(newFullPath))
                {
                    MessageBox.Show($"Файл с именем '{newFileName}' уже существует.",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CancelEdit();
                    return;
                }

                // Переименовываем файл на диске
                File.Move(originalFullPath, newFullPath);

                // Обновляем данные в объекте
                editingItem.FileName = newFileName;
                editingItem.FilePath = newFullPath;

                // Также обновляем оригинальное имя в parsed данных
                if (editingItem.CharacterList != null && editingItem.CharacterList.Count > 0)
                {
                    // Парсим новое имя для обновления данных
                    var newParsed = PartsOfImageName.ParseImageNameIntoParts(newFileName);
                    editingItem.Character = newParsed.Character;
                    editingItem.Author = newParsed.Author;
                    editingItem.Class = newParsed.Class;
                    editingItem.Number = newParsed.Number;
                    editingItem.Tags = newParsed.Tags;
                    editingItem.CharacterList = newParsed.CharacterList;
                    editingItem.AuthorList = newParsed.AuthorList;
                    editingItem.TagList = newParsed.TagList;
                }

                // Обновляем отображение в таблице
                currentDataGrid.Items.Refresh();

                // Восстанавливаем TextBlock
                RestoreTextBlock();

                // Обновляем превью, если оно активно
                if (PreviewFileName.Text.Contains(originalEditingFileName))
                {
                    PreviewFileName.Text = newFileName;
                }

                // Обновляем статистику, если нужно
                UpdateStatisticsAfterRename(originalEditingFileName, newFileName);

                // Очищаем редактирование
                editingTextBox = null;
                editingItem = null;
                originalEditingFileName = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при переименовании файла:\n{ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                CancelEdit();
            }
        }

        private void CancelEdit()
        {
            if (editingTextBox != null)
            {
                // Восстанавливаем TextBlock
                RestoreTextBlock();

                // Очищаем редактирование
                editingTextBox = null;
                editingItem = null;
                originalEditingFileName = null;
            }
        }

        private void RestoreTextBlock()
        {
            if (editingTextBox == null) return;

            // Находим родительский элемент и восстанавливаем TextBlock
            var parent = editingTextBox.Parent as FrameworkElement;
            if (parent != null)
            {
                // Проверяем, сохранили ли мы TextBlock в Tag
                if (parent.Tag is TextBlock originalTextBlock)
                {
                    if (parent is Border border)
                    {
                        border.Child = originalTextBlock;
                    }
                    else if (parent is ContentPresenter presenter)
                    {
                        presenter.Content = originalTextBlock;
                    }
                    else
                    {
                        var contentControl = FindVisualParent<ContentControl>(editingTextBox);
                        if (contentControl != null)
                        {
                            contentControl.Content = originalTextBlock;
                        }
                    }
                }
            }

            // Удаляем обработчики событий
            editingTextBox.KeyDown -= EditingTextBox_KeyDown;
            editingTextBox.LostFocus -= EditingTextBox_LostFocus;
        }

        private void UpdateStatisticsAfterRename(string oldFileName, string newFileName)
        {
            // Парсим старое и новое имена
            var oldParsed = PartsOfImageName.ParseImageNameIntoParts(oldFileName);
            var newParsed = PartsOfImageName.ParseImageNameIntoParts(newFileName);

            // Обновляем статистику персонажей
            UpdateCharacterStatisticsAfterRename(oldParsed, newParsed);

            // Обновляем статистику авторов
            UpdateAuthorStatisticsAfterRename(oldParsed, newParsed);

            // Обновляем статистику тегов
            UpdateTagStatisticsAfterRename(oldParsed, newParsed);
        }

        private void UpdateCharacterStatisticsAfterRename(PartsOfImageName oldParsed, PartsOfImageName newParsed)
        {
            // Уменьшаем счетчики для старых персонажей
            if (oldParsed?.CharacterList != null)
            {
                foreach (var character in oldParsed.CharacterList)
                {
                    if (characterStatistics.ContainsKey(character))
                    {
                        characterStatistics[character]--;
                        if (characterStatistics[character] <= 0)
                        {
                            characterStatistics.Remove(character);
                        }
                    }
                }
            }

            // Увеличиваем счетчики для новых персонажей
            if (newParsed?.CharacterList != null)
            {
                foreach (var character in newParsed.CharacterList)
                {
                    if (characterStatistics.ContainsKey(character))
                    {
                        characterStatistics[character]++;
                    }
                    else
                    {
                        characterStatistics[character] = 1;
                    }
                }
            }

            characterListNeedsUpdate = true;
        }

        private void UpdateAuthorStatisticsAfterRename(PartsOfImageName oldParsed, PartsOfImageName newParsed)
        {
            // Аналогично для авторов
            if (oldParsed?.AuthorList != null)
            {
                foreach (var author in oldParsed.AuthorList)
                {
                    if (authorStatistics.ContainsKey(author))
                    {
                        authorStatistics[author]--;
                        if (authorStatistics[author] <= 0)
                        {
                            authorStatistics.Remove(author);
                        }
                    }
                }
            }

            if (newParsed?.AuthorList != null)
            {
                foreach (var author in newParsed.AuthorList)
                {
                    if (authorStatistics.ContainsKey(author))
                    {
                        authorStatistics[author]++;
                    }
                    else
                    {
                        authorStatistics[author] = 1;
                    }
                }
            }

            authorListNeedsUpdate = true;
        }

        private void UpdateTagStatisticsAfterRename(PartsOfImageName oldParsed, PartsOfImageName newParsed)
        {
            // Аналогично для тегов
            if (oldParsed?.TagList != null)
            {
                foreach (var tag in oldParsed.TagList)
                {
                    if (tagStatistics.ContainsKey(tag))
                    {
                        tagStatistics[tag]--;
                        if (tagStatistics[tag] <= 0)
                        {
                            tagStatistics.Remove(tag);
                        }
                    }
                }
            }

            if (newParsed?.TagList != null)
            {
                foreach (var tag in newParsed.TagList)
                {
                    if (tagStatistics.ContainsKey(tag))
                    {
                        tagStatistics[tag]++;
                    }
                    else
                    {
                        tagStatistics[tag] = 1;
                    }
                }
            }

            tagListNeedsUpdate = true;
        }

        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }
        #endregion



        #region Главные методы
        private void ChooseImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GetImageDirectory();
        }

        private void IsSearchParent_Checked(object sender, RoutedEventArgs e)
        {
            FindDirectory();
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

                firstFilePath = Path.GetDirectoryName(fileDialog.FileName);
                FindDirectory();
            }
        }

        private void FindDirectory()
        {
            if (!string.IsNullOrEmpty(firstFilePath))
            {
                if (IsSearchParent != null && IsSearchParent.IsChecked == true)
                {
                    string parentPath = Directory.GetParent(firstFilePath)?.FullName;

                    if (!string.IsNullOrEmpty(parentPath) && Directory.Exists(parentPath))
                    {
                        directoryPath = parentPath;
                        SelectedFolder.Text = directoryPath;
                    }
                }
                else
                {
                    directoryPath = firstFilePath;
                    SelectedFolder.Text = directoryPath;
                }
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
                await LoadImagesAsync(directoryPath, isFullLoad, true, cancellationToken);

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
                sourceAllImageItems.Add(item);

                if (enabledClasses.Contains(item.Class))
                {
                    var classedItem = new ImageInfoItem
                    {
                        Index = classedAllImageItems.Count + 1,
                        FileName = item.FileName,
                        FilePath = item.FilePath,
                        Resolution = item.Resolution,
                        AspectRatio = item.AspectRatio,
                        FileSize = item.FileSize,
                        Format = item.Format,
                        Character = item.Character,
                        Author = item.Author,
                        Class = item.Class,
                        Number = item.Number,
                        Tags = item.Tags,
                        CharacterList = item.CharacterList,
                        AuthorList = item.AuthorList,
                        TagList = item.TagList,
                    };

                    classedAllImageItems.Add(classedItem);
                }

                UpdateCountText();
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
            sourceAllImageItems.Clear();
            sourceCharacterImageItems.Clear();
            sourceAuthorImageItems.Clear();
            sourceTagImageItems.Clear();

            classedAllImageItems.Clear();
            classedCharacterImageItems.Clear();
            classedAuthorImageItems.Clear();
            classedTagImageItems.Clear();

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

            includeCharacters.Clear();
            excludeCharacters.Clear();
            includeAuthors.Clear();
            excludeAuthors.Clear();
            includeTags.Clear();
            excludeTags.Clear();

            if (CharacterIncludePanel != null) CharacterIncludePanel.Children.Clear();
            if (CharacterExcludePanel != null) CharacterExcludePanel.Children.Clear();
            if (CharacterOtherPanel != null) CharacterOtherPanel.Children.Clear();

            if (AuthorIncludePanel != null) AuthorIncludePanel.Children.Clear();
            if (AuthorExcludePanel != null) AuthorExcludePanel.Children.Clear();
            if (AuthorOtherPanel != null) AuthorOtherPanel.Children.Clear();

            if (TagIncludePanel != null) TagIncludePanel.Children.Clear();
            if (TagExcludePanel != null) TagExcludePanel.Children.Clear();
            if (TagOtherPanel != null) TagOtherPanel.Children.Clear();

            Count.Text = "Всего изображений: 0";
            ClearImagePreview();
        }
        #endregion



        #region Переключатели ClassGroup
        private void Class1s_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandleClassClick("1s", e.ChangedButton);
        }
        private void Class2e_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandleClassClick("2e", e.ChangedButton);
        }

        private void Class3n_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandleClassClick("3n", e.ChangedButton);
        }

        private void Class4c_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandleClassClick("4c", e.ChangedButton);
        }

        private void Class5j_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandleClassClick("5j", e.ChangedButton);
        }

        private void Class6h_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandleClassClick("6h", e.ChangedButton);
        }

        private void HandleClassClick(string className, MouseButton mouseButton)
        {
            if (mouseButton == MouseButton.Right)
            {
                enabledClasses.Clear();
                enabledClasses.Add(className);
            }
            else if (mouseButton == MouseButton.Left)
            {
                if (enabledClasses.Contains(className))
                {
                    enabledClasses.Remove(className);
                }
                else enabledClasses.Add(className);
            }

            RefreshCurrentDataGrid();
        }

        private void RefreshCurrentDataGrid()
        {
            if (currentDataGrid == null) return;

            var sourceCollection = GetSourceCollectionForDataGrid(currentDataGrid);
            var classedCollection = GetClassedCollectionForDataGrid(currentDataGrid);

            if (sourceCollection == null || classedCollection == null) return;

            classedCollection.Clear();

            if (sourceCollection.Count == 0) return;

            var filteredItems = sourceCollection
                .Where(item => enabledClasses.Contains(item.Class))
                .ToList();

            for (int i = 0; i < filteredItems.Count; i++)
            {
                var classedItem = new ImageInfoItem
                {
                    Index = i + 1,
                    FileName = filteredItems[i].FileName,
                    FilePath = filteredItems[i].FilePath,
                    Resolution = filteredItems[i].Resolution,
                    AspectRatio = filteredItems[i].AspectRatio,
                    FileSize = filteredItems[i].FileSize,
                    Format = filteredItems[i].Format,
                    Character = filteredItems[i].Character,
                    Author = filteredItems[i].Author,
                    Class = filteredItems[i].Class,
                    Number = filteredItems[i].Number,
                    Tags = filteredItems[i].Tags,
                    CharacterList = filteredItems[i].CharacterList,
                    AuthorList = filteredItems[i].AuthorList,
                    TagList = filteredItems[i].TagList,
                };

                classedCollection.Add(classedItem);
            }

            UpdateCountText();
        }

        private ObservableCollection<ImageInfoItem> GetSourceCollectionForDataGrid(DataGrid dataGrid)
        {
            if (dataGrid == AllDataGrid) return sourceAllImageItems;
            if (dataGrid == CharacterDataGrid) return sourceCharacterImageItems;
            if (dataGrid == AuthorDataGrid) return sourceAuthorImageItems;
            if (dataGrid == TagDataGrid) return sourceTagImageItems;
            return null;
        }

        private ObservableCollection<ImageInfoItem> GetClassedCollectionForDataGrid(DataGrid dataGrid)
        {
            if (dataGrid == AllDataGrid) return classedAllImageItems;
            if (dataGrid == CharacterDataGrid) return classedCharacterImageItems;
            if (dataGrid == AuthorDataGrid) return classedAuthorImageItems;
            if (dataGrid == TagDataGrid) return classedTagImageItems;
            return null;
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
                currentDataGrid = dataGridToShow;

                RefreshCurrentDataGrid();
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



        #region Переключатели столбцов окна
        private void LeftSide_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeColumnVisibility(LeftColumn);
        }

        private void RightSide_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeColumnVisibility(RightColumn);
        }

        private void ChangeColumnVisibility(ColumnDefinition column)
        {
            if (column.Width == new GridLength(0, GridUnitType.Pixel) || column.Width == new GridLength(0, GridUnitType.Star))
            {
                column.Width = new GridLength(1, GridUnitType.Star);
            }
            else column.Width = new GridLength(0, GridUnitType.Pixel);

        }
        #endregion



        #region Общие методы для работы со статистикой
        private void UpdateList(Dictionary<string, int> statistics,
                               WrapPanel includePanel,
                               WrapPanel excludePanel,
                               WrapPanel otherPanel,
                               TextBlock includeHeaderText,
                               TextBlock excludeHeaderText,
                               ComboBox filterComboBox,
                               int amountTemplate,
                               Action<object, MouseButtonEventArgs> includeButtonClickHandler,
                               Action<object, MouseButtonEventArgs> excludeButtonClickHandler)
        {
            includePanel.Children.Clear();
            excludePanel.Children.Clear();
            otherPanel.Children.Clear();

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

            foreach (var item in filteredItems)
            {
                var button = CreateCategoryButton(item.Key, item.Value);

                // Проверяем, в какую панель добавить кнопку
                if (IsInIncludeSet(item.Key, includePanel))
                {
                    AddToIncludePanel(button, includePanel, includeButtonClickHandler, true);
                }
                else if (IsInExcludeSet(item.Key, excludePanel))
                {
                    AddToExcludePanel(button, excludePanel, excludeButtonClickHandler, false);
                }
                else
                {
                    AddToOtherPanel(button, otherPanel, includeButtonClickHandler, excludeButtonClickHandler);
                }
            }
        }

        private bool IsInIncludeSet(string itemName, WrapPanel includePanel)
        {
            if (includePanel == CharacterIncludePanel) return includeCharacters.Contains(itemName);
            if (includePanel == AuthorIncludePanel) return includeAuthors.Contains(itemName);
            if (includePanel == TagIncludePanel) return includeTags.Contains(itemName);
            return false;
        }

        private bool IsInExcludeSet(string itemName, WrapPanel excludePanel)
        {
            if (excludePanel == CharacterExcludePanel) return excludeCharacters.Contains(itemName);
            if (excludePanel == AuthorExcludePanel) return excludeAuthors.Contains(itemName);
            if (excludePanel == TagExcludePanel) return excludeTags.Contains(itemName);
            return false;
        }

        private TextBlock CreateCategoryButton(string name, int count)
        {
            return new TextBlock
            {
                Text = $"{name} [{count}]",
                Margin = new Thickness(5),
                Padding = new Thickness(8, 5, 8, 5),
                Background = new SolidColorBrush(Color.FromRgb(221, 223, 233)),
                FontSize = 13,
                Cursor = Cursors.Hand,
                Tag = name
            };
        }

        private void AddToIncludePanel(TextBlock button, WrapPanel panel, Action<object, MouseButtonEventArgs> clickHandler, bool isInclude)
        {
            button.Background = isInclude ?
                new SolidColorBrush(Color.FromRgb(210, 243, 211)) : // светло-зеленый для включаемых
                new SolidColorBrush(Color.FromRgb(243, 210, 210));  // светло-красный для исключаемых

            // Левая кнопка - снимает выбор
            button.MouseLeftButtonDown += (s, e) => clickHandler(s, e);

            // Правая кнопка на включаемых - перемещает в исключаемые
            if (isInclude)
            {
                button.MouseRightButtonDown += (s, e) =>
                {
                    // Создаем аргументы для перемещения в исключаемые
                    if (panel == CharacterIncludePanel)
                        ExcludeCharacterButton_Click(s, e);
                    else if (panel == AuthorIncludePanel)
                        ExcludeAuthorButton_Click(s, e);
                    else if (panel == TagIncludePanel)
                        ExcludeTagButton_Click(s, e);
                    e.Handled = true;
                };
            }
            // Правая кнопка на исключаемых - перемещает во включаемые
            else
            {
                button.MouseRightButtonDown += (s, e) =>
                {
                    // Создаем аргументы для перемещения во включаемые
                    if (panel == CharacterExcludePanel)
                        IncludeCharacterButton_Click(s, e);
                    else if (panel == AuthorExcludePanel)
                        IncludeAuthorButton_Click(s, e);
                    else if (panel == TagExcludePanel)
                        IncludeTagButton_Click(s, e);
                    e.Handled = true;
                };
            }

            button.MouseEnter += (s, e) =>
            {
                button.Background = isInclude ?
                    new SolidColorBrush(Color.FromRgb(186, 243, 186)) : // зеленый при наведении
                    new SolidColorBrush(Color.FromRgb(243, 183, 183)); // красный при наведении
            };

            button.MouseLeave += (s, e) =>
            {
                button.Background = isInclude ?
                    new SolidColorBrush(Color.FromRgb(210, 243, 211)) : // светло-зеленый
                    new SolidColorBrush(Color.FromRgb(243, 210, 210));  // светло-красный
            };

            panel.Children.Add(button);
        }

        private void AddToExcludePanel(TextBlock button, WrapPanel panel, Action<object, MouseButtonEventArgs> clickHandler, bool isInclude)
        {
            AddToIncludePanel(button, panel, clickHandler, isInclude);
        }

        private void AddToOtherPanel(TextBlock button, WrapPanel panel,
            Action<object, MouseButtonEventArgs> includeClickHandler,
            Action<object, MouseButtonEventArgs> excludeClickHandler)
        {
            // Обработчик для левой кнопки
            button.MouseLeftButtonDown += (s, e) =>
            {
                includeClickHandler(s, e);
            };

            // Обработчик для правой кнопки
            button.MouseRightButtonDown += (s, e) =>
            {
                excludeClickHandler(s, e);
                e.Handled = true; // Предотвращаем всплытие события
            };

            button.MouseEnter += (s, e) =>
            {
                button.Background = new SolidColorBrush(Color.FromRgb(174, 194, 235));
            };

            button.MouseLeave += (s, e) =>
            {
                button.Background = new SolidColorBrush(Color.FromRgb(221, 223, 233));
            };

            panel.Children.Add(button);
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
                                        ObservableCollection<ImageInfoItem> targetSourceCollection,
                                        Func<ImageInfoItem, bool> filterPredicate,
                                        OneMenuOption showDataGridControl,
                                        DataGrid targetDataGrid)
        {
            targetSourceCollection.Clear();

            var filteredItems = sourceAllImageItems
                .Where(filterPredicate)
                .ToList();

            foreach (var item in filteredItems)
            {
                targetSourceCollection.Add(item);
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



        #region Клики
        private void IncludeCharacterButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textButton && textButton.Tag is string characterName)
            {
                // Обрабатываем клик
                HandleCharacterClick(characterName, true);
            }
        }

        private void ExcludeCharacterButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textButton && textButton.Tag is string characterName)
            {
                // Обрабатываем клик
                HandleCharacterClick(characterName, false);
            }
        }

        // Новый метод для обработки кликов по персонажам
        private void HandleCharacterClick(string characterName, bool isInclude)
        {
            if (isInclude)
            {
                // Если элемент уже во включаемых - удаляем его
                if (includeCharacters.Contains(characterName))
                {
                    includeCharacters.Remove(characterName);
                }
                // Если элемент в исключаемых - перемещаем во включаемые
                else if (excludeCharacters.Contains(characterName))
                {
                    excludeCharacters.Remove(characterName);
                    includeCharacters.Add(characterName);
                }
                // Если элемент не выбран - добавляем во включаемые
                else
                {
                    includeCharacters.Add(characterName);
                }
            }
            else
            {
                // Если элемент уже в исключаемых - удаляем его
                if (excludeCharacters.Contains(characterName))
                {
                    excludeCharacters.Remove(characterName);
                }
                // Если элемент во включаемых - перемещаем в исключаемые
                else if (includeCharacters.Contains(characterName))
                {
                    includeCharacters.Remove(characterName);
                    excludeCharacters.Add(characterName);
                }
                // Если элемент не выбран - добавляем в исключаемые
                else
                {
                    excludeCharacters.Add(characterName);
                }
            }

            UpdateCharacterList();
            ApplyCharacterFilter();
        }

        private void IncludeAuthorButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textButton && textButton.Tag is string authorName)
            {
                HandleAuthorClick(authorName, true);
            }
        }

        private void ExcludeAuthorButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textButton && textButton.Tag is string authorName)
            {
                HandleAuthorClick(authorName, false);
            }
        }

        private void HandleAuthorClick(string authorName, bool isInclude)
        {
            // Такая же логика как для персонажей
            if (isInclude)
            {
                if (includeAuthors.Contains(authorName))
                {
                    includeAuthors.Remove(authorName);
                }
                else if (excludeAuthors.Contains(authorName))
                {
                    excludeAuthors.Remove(authorName);
                    includeAuthors.Add(authorName);
                }
                else
                {
                    includeAuthors.Add(authorName);
                }
            }
            else
            {
                if (excludeAuthors.Contains(authorName))
                {
                    excludeAuthors.Remove(authorName);
                }
                else if (includeAuthors.Contains(authorName))
                {
                    includeAuthors.Remove(authorName);
                    excludeAuthors.Add(authorName);
                }
                else
                {
                    excludeAuthors.Add(authorName);
                }
            }

            UpdateAuthorList();
            ApplyAuthorFilter();
        }
        private void IncludeTagButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textButton && textButton.Tag is string tagName)
            {
                HandleTagClick(tagName, true);
            }
        }

        private void ExcludeTagButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textButton && textButton.Tag is string tagName)
            {
                HandleTagClick(tagName, false);
            }
        }

        private void HandleTagClick(string tagName, bool isInclude)
        {
            // Такая же логика как для персонажей
            if (isInclude)
            {
                if (includeTags.Contains(tagName))
                {
                    includeTags.Remove(tagName);
                }
                else if (excludeTags.Contains(tagName))
                {
                    excludeTags.Remove(tagName);
                    includeTags.Add(tagName);
                }
                else
                {
                    includeTags.Add(tagName);
                }
            }
            else
            {
                if (excludeTags.Contains(tagName))
                {
                    excludeTags.Remove(tagName);
                }
                else if (includeTags.Contains(tagName))
                {
                    includeTags.Remove(tagName);
                    excludeTags.Add(tagName);
                }
                else
                {
                    excludeTags.Add(tagName);
                }
            }

            UpdateTagList();
            ApplyTagFilter();
        }




        private void ApplyCharacterFilter()
        {
            if (includeCharacters.Count == 0 && excludeCharacters.Count == 0)
            {
                sourceCharacterImageItems.Clear();
                classedCharacterImageItems.Clear();
                CharacterDataGrid.Items.Refresh();

                // Обновляем счетчик
                UpdateCountText();
                return;
            }

            // Фильтруем изображения по выбранным персонажам
            var filteredItems = sourceAllImageItems
                .Where(item =>
                {
                    // Должны содержать ВСЕ включенные персонажи
                    if (includeCharacters.Count > 0)
                    {
                        if (item.CharacterList == null || !includeCharacters.All(inc => item.CharacterList.Contains(inc)))
                            return false;
                    }

                    // Не должны содержать НИ ОДНОГО исключенного персонажа
                    if (excludeCharacters.Count > 0)
                    {
                        if (item.CharacterList != null && excludeCharacters.Any(exc => item.CharacterList.Contains(exc)))
                            return false;
                    }

                    return true;
                })
                .ToList();

            UpdateFilteredCollection(filteredItems, sourceCharacterImageItems, ShowCharacterDataGrid, CharacterDataGrid);
        }

        private void ApplyAuthorFilter()
        {
            if (includeAuthors.Count == 0 && excludeAuthors.Count == 0)
            {
                sourceAuthorImageItems.Clear();
                classedAuthorImageItems.Clear();
                AuthorDataGrid.Items.Refresh();

                // Обновляем счетчик
                UpdateCountText();
                return;
            }

            var filteredItems = sourceAllImageItems
                .Where(item =>
                {
                    if (includeAuthors.Count > 0)
                    {
                        if (item.AuthorList == null || !includeAuthors.All(inc => item.AuthorList.Contains(inc)))
                            return false;
                    }

                    if (excludeAuthors.Count > 0)
                    {
                        if (item.AuthorList != null && excludeAuthors.Any(exc => item.AuthorList.Contains(exc)))
                            return false;
                    }

                    return true;
                })
                .ToList();

            UpdateFilteredCollection(filteredItems, sourceAuthorImageItems, ShowAuthorDataGrid, AuthorDataGrid);
        }

        private void ApplyTagFilter()
        {
            if (includeTags.Count == 0 && excludeTags.Count == 0)
            {
                sourceTagImageItems.Clear();
                classedTagImageItems.Clear();
                TagDataGrid.Items.Refresh();

                // Обновляем счетчик
                UpdateCountText();
                return;
            }

            var filteredItems = sourceAllImageItems
                .Where(item =>
                {
                    if (includeTags.Count > 0)
                    {
                        if (item.TagList == null || !includeTags.All(inc => item.TagList.Contains(inc)))
                            return false;
                    }

                    if (excludeTags.Count > 0)
                    {
                        if (item.TagList != null && excludeTags.Any(exc => item.TagList.Contains(exc)))
                            return false;
                    }

                    return true;
                })
                .ToList();

            UpdateFilteredCollection(filteredItems, sourceTagImageItems, ShowTagDataGrid, TagDataGrid);
        }

        private void UpdateFilteredCollection(List<ImageInfoItem> filteredItems,
            ObservableCollection<ImageInfoItem> targetCollection,
            OneMenuOption showDataGridControl,
            DataGrid targetDataGrid)
        {
            targetCollection.Clear();

            foreach (var item in filteredItems)
            {
                targetCollection.Add(item);
            }

            // Показываем соответствующую таблицу
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
            UpdateList(characterStatistics,
                       CharacterIncludePanel,
                       CharacterExcludePanel,
                       CharacterOtherPanel,
                       CharacterIncludeHeaderText,
                       CharacterExcludeHeaderText,
                       CharacterFilterTypeComboBox,
                       CharacterAmountTemplate,
                       IncludeCharacterButton_Click,
                       ExcludeCharacterButton_Click);
        }

        private void CharacterButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textButton && textButton.Tag is string characterName)
            {
                FilterAndShowItems(characterName, sourceCharacterImageItems,
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
            UpdateList(authorStatistics,
                       AuthorIncludePanel,
                       AuthorExcludePanel,
                       AuthorOtherPanel,
                       authorIncludeHeaderText,
                       AuthorExcludeHeaderText,
                       AuthorFilterTypeComboBox,
                       AuthorAmountTemplate,
                       IncludeAuthorButton_Click,
                       ExcludeAuthorButton_Click);
        }

        private void AuthorButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textButton && textButton.Tag is string authorName)
            {
                FilterAndShowItems(authorName, sourceAuthorImageItems,
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
            UpdateList(tagStatistics,
                       TagIncludePanel,
                       TagExcludePanel,
                       TagOtherPanel,
                       TagIncludeHeaderText,
                       TagExcludeHeaderText,
                       TagFilterTypeComboBox,
                       TagAmountTemplate,
                       IncludeTagButton_Click,
                       ExcludeTagButton_Click);
        }

        private void TagButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textButton && textButton.Tag is string tagName)
            {
                FilterAndShowItems(tagName, sourceTagImageItems, 
                    i => i.TagList?.Contains(tagName) == true, 
                    ShowTagDataGrid, TagDataGrid);
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



        #region Дополнительные действия
        private void CreateRatioEdgeFolders_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Создаем подпапки
            string portraitPath = Path.Combine(directoryPath, "Portrait");
            string landscapePath = Path.Combine(directoryPath, "Landscape");
            string squarePath = Path.Combine(directoryPath, "Square");

            // Используем базовый класс
            var result = FolderOperation.MoveFilesToFolders(
                classedAllImageItems,
                directoryPath,
                (item, index) =>
                {
                    if (double.TryParse(item.AspectRatio.Replace('.', ','), out double aspectRatio))
                    {
                        double edge = RatioEdgeTemplate;

                        if (aspectRatio < (1 - edge))
                            return landscapePath;
                        else if (aspectRatio > (1 + edge))
                            return portraitPath;
                        else
                            return squarePath;
                    }
                    return squarePath; // По умолчанию
                });

            ShowResultMessage("Сортировка по соотношению сторон завершена!", result);
        }

        private void CreateMaxAmountFolders_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MaxAmountTemplate <= 0)
            {
                MessageBox.Show("Максимальное количество файлов в папке должно быть больше 0", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int totalFiles = classedAllImageItems.Count;
            int foldersCount = (int)Math.Ceiling((double)totalFiles / MaxAmountTemplate);

            // Создаем папки заранее
            List<string> folders = new List<string>();
            for (int i = 0; i < foldersCount; i++)
            {
                int startNum = i * MaxAmountTemplate + 1;
                int endNum = Math.Min((i + 1) * MaxAmountTemplate, totalFiles);
                string folderName = $"{startNum}-{endNum}";
                string folderPath = Path.Combine(directoryPath, folderName);

                Directory.CreateDirectory(folderPath);
                folders.Add(folderPath);
            }

            // Используем базовый класс
            var result = FolderOperation.MoveFilesToFolders(
                classedAllImageItems,
                directoryPath,
                (item, index) =>
                {
                    int folderIndex = index / MaxAmountTemplate;
                    return folders[folderIndex];
                });

            ShowResultMessage($"Сортировка по количеству файлов завершена!\nСоздано папок: {foldersCount}", result);
        }

        private void ShowResultMessage(string title, (int movedCount, int errorCount, int alreadySortedCount) result)
        {
            MessageBox.Show($"{title}\n\n" +
                               $"Перемещено файлов: {result.movedCount}\n" +
                               $"Уже отсортировано: {result.alreadySortedCount}\n" +
                               $"Ошибок: {result.errorCount}\n\n" +
                               $"Сейчас таблица будет очищена",
                               "Информация",
                               MessageBoxButton.OK);
            Clean();
        }
        #endregion



        #region Сортировка тегов
        private async void SortTags_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Загружаем файлы для обработки
            var items = await LoadImageNameFilesAsync(moveImageFiles);

            // Определяем порядок сортировки для тегов с буквой 'f'
            var tagOrder = new List<string> { "fb00", "fbe11", "fa55", "fh1p", "fd1ck" };

            // Обрабатываем каждый файл
            foreach (var item in items)
            {
                string fileName = Path.GetFileNameWithoutExtension(item.OriginalName);
                string extension = Path.GetExtension(item.OriginalName);

                // Разделяем имя и теги
                int atIndex = fileName.IndexOf('@');
                if (atIndex == -1)
                {
                    // Нет тегов, оставляем имя как есть
                    item.NewName = item.OriginalName;
                    continue;
                }

                string baseName = fileName.Substring(0, atIndex).Trim();
                string tagsString = fileName.Substring(atIndex + 1).Trim();

                if (string.IsNullOrEmpty(tagsString))
                {
                    // Пустые теги, оставляем имя как есть
                    item.NewName = item.OriginalName;
                    continue;
                }

                // Разделяем теги на массив
                string[] tags = tagsString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Находим позицию первого f-тега
                int firstFTagIndex = -1;
                for (int i = 0; i < tags.Length; i++)
                {
                    if (tags[i].StartsWith("f", StringComparison.OrdinalIgnoreCase))
                    {
                        firstFTagIndex = i;
                        break;
                    }
                }

                // Если нет f-тегов, оставляем имя как есть
                if (firstFTagIndex == -1)
                {
                    item.NewName = item.OriginalName;
                    continue;
                }

                // Собираем теги до первого f-тега
                List<string> beforeFTags = new List<string>();
                for (int i = 0; i < firstFTagIndex; i++)
                {
                    beforeFTags.Add(tags[i]);
                }

                // Собираем f-теги
                List<string> fTags = new List<string>();
                List<string> afterTags = new List<string>();
                bool collectingFTags = false;

                for (int i = firstFTagIndex; i < tags.Length; i++)
                {
                    if (tags[i].StartsWith("f", StringComparison.OrdinalIgnoreCase))
                    {
                        fTags.Add(tags[i]);
                        collectingFTags = true;
                    }
                    else if (collectingFTags)
                    {
                        // Как только встретили не f-тег после f-тегов, перестаем собирать f-теги
                        // и добавляем остальные теги в afterTags
                        for (int j = i; j < tags.Length; j++)
                        {
                            afterTags.Add(tags[j]);
                        }
                        break;
                    }
                }

                // Если после f-тегов нет других тегов
                if (!collectingFTags || afterTags.Count == 0)
                {
                    // Добавляем все теги после первого f-тега как другие теги
                    for (int i = firstFTagIndex + fTags.Count; i < tags.Length; i++)
                    {
                        afterTags.Add(tags[i]);
                    }
                }

                // Сортируем f-теги по заданному порядку
                fTags = fTags.OrderBy(tag =>
                {
                    int index = tagOrder.FindIndex(orderedTag =>
                        string.Equals(orderedTag, tag, StringComparison.OrdinalIgnoreCase));
                    return index == -1 ? int.MaxValue : index; // Теги не из списка идут в конец
                }).ToList();

                // Объединяем все теги в правильном порядке
                List<string> allTags = new List<string>();
                allTags.AddRange(beforeFTags);
                allTags.AddRange(fTags);
                allTags.AddRange(afterTags);

                // Формируем новое имя файла
                string newTagsString = string.Join(" ", allTags);
                string newFileName = $"{baseName} @{newTagsString}{extension}";

                // Обновляем запись
                item.NewName = newFileName;
            }

            // Обновляем UI
            moveImageFiles.Clear();
            foreach (var item in items)
            {
                moveImageFiles.Add(item);
            }

            // Показываем таблицу с результатами
            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = ShowMoveDataGrid
            };

            ShowMoveDataGrid.RaiseEvent(args);
            ShowCurrentDataGrid(MoveDataGrid);

            // Сразу применяем переименование
            RenameSave(moveImageFiles);
        }
        #endregion



        #region Перенос авторов в квадратные скобки
        private async void MoveAuthorsToBrackets_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Загружаем файлы для обработки
            var items = await LoadImageNameFilesAsync(replaceImageFiles);

            // Обрабатываем каждый файл
            foreach (var item in items)
            {
                string fileName = Path.GetFileNameWithoutExtension(item.OriginalName);
                string extension = Path.GetExtension(item.OriginalName);

                // Используем Regex для поиска слова Work (регистронезависимо)
                var match = Regex.Match(fileName, @"\s+Work\s+", RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    // Также проверяем, если "Work" в самом конце
                    if (Regex.IsMatch(fileName, @"\s+Work$", RegexOptions.IgnoreCase))
                    {
                        // Находим позицию последнего "Work"
                        int workIndex = fileName.LastIndexOf("Work", StringComparison.OrdinalIgnoreCase);
                        if (workIndex > 0)
                        {
                            // Проверяем, что перед "Work" есть пробел или это начало строки
                            if (workIndex == 0 || fileName[workIndex - 1] == ' ')
                            {
                                // Извлекаем часть до "Work" - это автор
                                string authorPartWork = fileName.Substring(0, workIndex).Trim();

                                // Формируем новое имя
                                string newFileName = $"Zzz.Original [{authorPartWork}]{extension}";
                                item.NewName = newFileName;
                                continue;
                            }
                        }
                    }

                    // Нет слова "Work", оставляем как есть
                    item.NewName = item.OriginalName;
                    continue;
                }

                // Нашли "Work" в середине имени
                int workIndexMiddle = match.Index;
                string authorPart = fileName.Substring(0, workIndexMiddle).Trim();

                // Извлекаем часть после "Work" (длина "Work" + пробелы)
                string restPart = fileName.Substring(workIndexMiddle + match.Length).Trim();

                // Убираем возможные двойные пробелы в оставшейся части
                restPart = Regex.Replace(restPart, @"\s+", " ").Trim();

                // Проверяем, что автор не пустой и не содержит уже квадратные скобки
                if (!string.IsNullOrEmpty(authorPart) &&
                    !authorPart.StartsWith("[") &&
                    !authorPart.EndsWith("]"))
                {
                    // Формируем новое имя
                    string newFileName;

                    if (string.IsNullOrEmpty(restPart))
                    {
                        newFileName = $"Zzz.Original [{authorPart}]{extension}";
                    }
                    else
                    {
                        newFileName = $"Zzz.Original [{authorPart}] {restPart}{extension}";
                    }

                    item.NewName = newFileName;
                }
                else
                {
                    // Автор уже в скобках или что-то не так
                    item.NewName = item.OriginalName;
                }
            }

            // Обновляем UI
            replaceImageFiles.Clear();
            foreach (var item in items)
            {
                replaceImageFiles.Add(item);
            }

            // Показываем таблицу с результатами
            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = ShowReplaceDataGrid
            };

            ShowReplaceDataGrid.RaiseEvent(args);
            ShowCurrentDataGrid(ReplaceDataGrid);

            // Сразу применяем переименование
            RenameSave(replaceImageFiles);
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
                string fullPath = item.FilePath;

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

            if (selectedItem != null)
            {
                string fullPath = selectedItem.FilePath; 

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
                    MessageBox.Show($"Файл не найден:\n{fullPath}",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        #endregion



        #region Вспомогательные
        private void UpdateCountText()
        {
            if (currentDataGrid == null) return;

            var classedCollection = GetClassedCollectionForDataGrid(currentDataGrid);
            if (classedCollection == null) return;

            Count.Text = $"Всего изображений: {classedCollection.Count}";
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
