using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Reviser
{
    public partial class OneMenuOption : UserControl
    {
        #region Свойства    
        public static readonly DependencyProperty BackColorProperty = DependencyProperty.Register("BackColor", typeof(Brush), typeof(OneMenuOption), new PropertyMetadata(Brushes.White));
        public static readonly DependencyProperty StartColorProperty = DependencyProperty.Register("StartColor", typeof(Brush), typeof(OneMenuOption), new PropertyMetadata(Brushes.White));
        public static readonly DependencyProperty ClickedColorProperty = DependencyProperty.Register("ClickedColor", typeof(Brush), typeof(OneMenuOption), new PropertyMetadata(null));
        public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register("TextColor", typeof(Brush), typeof(OneMenuOption), new PropertyMetadata(Brushes.Black));
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(string), typeof(OneMenuOption), new PropertyMetadata(null));   
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(OneMenuOption), new PropertyMetadata(false));
        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register("GroupName", typeof(string), typeof(OneMenuOption), new PropertyMetadata(""));

        public Brush BackColor
        {
            get { return (Brush)GetValue(BackColorProperty); }
            set { SetValue(BackColorProperty, value); }
        }
        public Brush StartColor
        {
            get { return (Brush)GetValue(StartColorProperty); }
            set { SetValue(StartColorProperty, value); }
        }
        public Brush ClickedColor
        {
            get { return (Brush)GetValue(ClickedColorProperty); }
            set { SetValue(ClickedColorProperty, value); }
        }
        public Brush TextColor
        {
            get { return (Brush)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }
        public string Command
        {
            get { return (string)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }
        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }
        #endregion

        private bool isLoaded = false;



        #region Конструктор
        public OneMenuOption()
        {
            InitializeComponent();
            DataContext = this;

            ClickedColor = new SolidColorBrush(Color.FromRgb(174, 194, 235));
        }
        #endregion



        #region Действия мыши
        private void OneMenuOption_Loaded(object sender, RoutedEventArgs e)
        {
            isLoaded = true;
            this.UpdateVisualState(this);
        }

        private void OneMenuOption_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is OneMenuOption oneMenuOption && oneMenuOption.isLoaded)
            {
                if (!oneMenuOption.IsChecked)
                    oneMenuOption.IsChecked = !oneMenuOption.IsChecked;

                oneMenuOption.UncheckOthers(oneMenuOption.GroupName);
                oneMenuOption.UpdateVisualState(oneMenuOption);
            }
        }
        #endregion



        #region Смена выбранного элемента
        private void UncheckOthers(string groupName)
        {
            if (!string.IsNullOrEmpty(groupName))
            {
                if (Parent is Panel parentPanel)
                {
                    foreach (var child in parentPanel.Children)
                    {
                        if (child is OneMenuOption otherOneMenuOption &&
                            otherOneMenuOption != this &&
                            otherOneMenuOption.GroupName == groupName)
                        {
                            otherOneMenuOption.IsChecked = false;

                            UpdateVisualState(otherOneMenuOption);
                        }
                    }
                }
            }
        }

        private void UpdateVisualState(OneMenuOption oneMenuOption)
        {
            if (oneMenuOption.IsChecked)
                oneMenuOption.BackColor = oneMenuOption.ClickedColor;
            else oneMenuOption.BackColor = oneMenuOption.StartColor;
        }
        #endregion
    }
}
