using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Reviser
{
    public partial class Controller : UserControl
    {
        #region Свойства
        public static readonly DependencyProperty BackColorProperty = DependencyProperty.Register("BackColor", typeof(Brush), typeof(Controller), new PropertyMetadata(Brushes.White));
        public static readonly DependencyProperty StartColorProperty = DependencyProperty.Register("StartColor", typeof(Brush), typeof(Controller), new PropertyMetadata(Brushes.White));
        public static readonly DependencyProperty HoverColorProperty = DependencyProperty.Register("HoverColor", typeof(Brush), typeof(Controller), new PropertyMetadata(null));
        public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register("TextColor", typeof(Brush), typeof(Controller), new PropertyMetadata(Brushes.Black));
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(string), typeof(Controller), new PropertyMetadata(null));
        public static readonly DependencyProperty PurposeProperty = DependencyProperty.Register("Purpose", typeof(string), typeof(Controller), new PropertyMetadata(null));

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
            set
            { 
                SetValue(HoverColorProperty, value);
                Controller_MouseEnter(this, null);
            }
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
        #endregion



        #region Конструктор
        public Controller()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Controller(string title)
        {
            InitializeComponent();
            DataContext = this;

            Command = title;
        }
        #endregion



        #region Действия мыши
        private void Controller_Loaded(object sender, RoutedEventArgs e)
        {
            switch (Purpose)
            {
                case "Accept":
                    StartColor = new SolidColorBrush(Color.FromRgb(176, 233, 190));
                    HoverColor = new SolidColorBrush(Color.FromRgb(138, 237, 164));
                    break;

                case "Action":
                    StartColor = new SolidColorBrush(Color.FromRgb(174, 194, 235));
                    HoverColor = new SolidColorBrush(Color.FromRgb(135, 171, 244));
                    break;

                case "Delete":
                    StartColor = new SolidColorBrush(Color.FromRgb(235, 172, 178));
                    HoverColor = new SolidColorBrush(Color.FromRgb(240, 130, 141));
                    break;

                case "Exit":
                    StartColor = new SolidColorBrush(Color.FromRgb(119, 119, 127));
                    HoverColor = new SolidColorBrush(Color.FromRgb(102, 102, 110));
                    TextColor = Brushes.White; break;
            }

            BackColor = StartColor;
        }


        private void Controller_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ((Controller)sender).Figure.Visibility = Visibility.Visible;
        }


        private void Controller_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ((Controller)sender).Figure.Visibility = Visibility.Hidden; 
        }

        private void Controller_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (((Controller)sender).IsEnabled &&
                ((Controller)sender).HoverColor != Brushes.Black &&
                ((Controller)sender).StartColor != ((Controller)sender).HoverColor)
                Animation.CreateColorAnimation((Controller)sender, BackColor, ((SolidColorBrush)HoverColor).Color, 0.35);
        }

        private void Controller_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (((Controller)sender).IsEnabled &&
                ((Controller)sender).HoverColor != Brushes.Black &&
                ((Controller)sender).StartColor != ((Controller)sender).HoverColor)
                Animation.CreateColorAnimation((Controller)sender, BackColor, ((SolidColorBrush)StartColor).Color, 0.35);

            ((Controller)sender).Figure.Visibility = Visibility.Visible;
        }
        #endregion
    }
}
