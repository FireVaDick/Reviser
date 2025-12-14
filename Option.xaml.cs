using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Reviser
{
    public partial class Option : UserControl
    {
        #region Свойства
        public static readonly DependencyProperty GlobalWidthProperty = DependencyProperty.Register("GlobalWidth", typeof(double), typeof(Option), new PropertyMetadata(null));
        public static readonly DependencyProperty FigureWidthProperty = DependencyProperty.Register("FigurelWidth", typeof(double), typeof(Option), new PropertyMetadata(null));
        public static readonly DependencyProperty BackColorProperty = DependencyProperty.Register("BackColor", typeof(Brush), typeof(Option), new PropertyMetadata(null));
        public static readonly DependencyProperty StartColorProperty = DependencyProperty.Register("StartColor", typeof(Brush), typeof(Option), new PropertyMetadata(null));
        public static readonly DependencyProperty HoverColorProperty = DependencyProperty.Register("HoverColor", typeof(Brush), typeof(Option), new PropertyMetadata(null));
        public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register("TextColor", typeof(Brush), typeof(Option), new PropertyMetadata(null));
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(string), typeof(Option), new PropertyMetadata(null));
        public static readonly DependencyProperty PurposeProperty = DependencyProperty.Register("Purpose", typeof(string), typeof(Option), new PropertyMetadata(null));
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(Option), new PropertyMetadata(false, OnIsCheckedChanged));
        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register("GroupName", typeof(string), typeof(Option), new PropertyMetadata(""));
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Option));

        public double GlobalWidth
        {
            get { return (double)GetValue(GlobalWidthProperty); }
            set { SetValue(GlobalWidthProperty, value); }
        }

        public double FigureWidth
        {
            get { return (double)GetValue(FigureWidthProperty); }
            set { SetValue(FigureWidthProperty, value); }
        }

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

        public Brush HoverColor
        {
            get { return (Brush)GetValue(HoverColorProperty); }
            set { SetValue(HoverColorProperty, value); }
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

        public string Purpose
        {
            get { return (string)GetValue(PurposeProperty); }
            set { SetValue(PurposeProperty, value); }
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

        public event RoutedEventHandler Checked
        {
            add => AddHandler(CheckedEvent, value);
            remove => RemoveHandler(CheckedEvent, value);
        }

        public event RoutedEventHandler Unchecked
        {
            add => AddHandler(UncheckedEvent, value);
            remove => RemoveHandler(UncheckedEvent, value);
        }

        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }
        #endregion

        #region События
        public static readonly RoutedEvent CheckedEvent = EventManager.RegisterRoutedEvent("Checked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Option));
        public static readonly RoutedEvent UncheckedEvent = EventManager.RegisterRoutedEvent("Unchecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Option));
        #endregion

        private bool isMouseOver = false;
        private bool isLoaded = false;



        public Option()
        {
            InitializeComponent();
            DataContext = this;

            TextColor = Brushes.Black;
            BackColor = StartColor = Brushes.White;
        }

        private void Option_Loaded(object sender, RoutedEventArgs e)
        {
            isLoaded = true;
            UpdateVisualState();
        }

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Option option && option.isLoaded)
            {
                bool newValue = (bool)e.NewValue;
                bool oldValue = (bool)e.OldValue;

                if (newValue != oldValue)
                {
                    if (newValue)
                    {
                        // Если установили IsChecked = true, сбрасываем другие опции в группе
                        if (!string.IsNullOrEmpty(option.GroupName))
                        {
                            option.UncheckOtherOptionsInGroup();
                        }

                        option.RaiseEvent(new RoutedEventArgs(CheckedEvent));
                    }
                    else
                    {
                        option.RaiseEvent(new RoutedEventArgs(UncheckedEvent));
                    }

                    option.UpdateVisualState();
                }
            }
        }

        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Option option && option.isLoaded)
            {
                // При изменении визуальных свойств обновляем состояние
                option.UpdateVisualState();
            }
        }

        private void UncheckOtherOptionsInGroup()
        {
            if (Parent is Panel parentPanel)
            {
                foreach (var child in parentPanel.Children)
                {
                    if (child is Option otherOption &&
                        otherOption != this &&
                        otherOption.GroupName == GroupName)
                    {
                        otherOption.IsChecked = false;
                    }
                }
            }
        }

        private void UpdateVisualState()
        {
            if (IsChecked)
            {
                BackColor = new SolidColorBrush(Color.FromRgb(174, 194, 235));
                //BackColor = new SolidColorBrush(Color.FromRgb(135, 171, 244));
            }
            else if (isMouseOver)
            {
                
            }
            else
            {
                BackColor = Brushes.White;
            }
        }

        private void Option_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsChecked)
            {
                IsChecked = true;
            }

            // Вызываем событие Click
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }

        private void Option_MouseEnter(object sender, MouseEventArgs e)
        {
            isMouseOver = true;
            UpdateVisualState();
        }

        private void Option_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseOver = false;
            UpdateVisualState();
        }
    }
}
