using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Reviser
{
    public partial class AnyMenuOption : UserControl
    {
        #region Свойства    
        public static readonly DependencyProperty BackColorProperty = DependencyProperty.Register("BackColor", typeof(Brush), typeof(AnyMenuOption), new PropertyMetadata(Brushes.White));
        public static readonly DependencyProperty StartColorProperty = DependencyProperty.Register("StartColor", typeof(Brush), typeof(AnyMenuOption), new PropertyMetadata(Brushes.White));
        public static readonly DependencyProperty ClickedColorProperty = DependencyProperty.Register("ClickedColor", typeof(Brush), typeof(AnyMenuOption), new PropertyMetadata(null));
        public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register("TextColor", typeof(Brush), typeof(AnyMenuOption), new PropertyMetadata(Brushes.Black));
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(string), typeof(AnyMenuOption), new PropertyMetadata(null));   
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(AnyMenuOption), new PropertyMetadata(true));
        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register("GroupName", typeof(string), typeof(AnyMenuOption), new PropertyMetadata(""));

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
        public AnyMenuOption()
        {
            InitializeComponent();
            DataContext = this;
        }
        #endregion



        #region Действия мыши
        private void AnyMenuOption_Loaded(object sender, RoutedEventArgs e)
        {
            isLoaded = true;
            this.UpdateVisualState(this);
        }

        private void AnyMenuOption_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is AnyMenuOption anyMenuOption && anyMenuOption.isLoaded)
            {
                anyMenuOption.IsChecked = !anyMenuOption.IsChecked;

                anyMenuOption.UpdateVisualState(anyMenuOption);
            }
        }
        #endregion



        #region Смена выбранного элемента
        private void UpdateVisualState(AnyMenuOption anyMenuOption)
        {
            if (anyMenuOption.IsChecked)
                anyMenuOption.BackColor = anyMenuOption.ClickedColor;
            else anyMenuOption.BackColor = anyMenuOption.StartColor;
        }
        #endregion
    }
}
