using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Reviser
{
    class Animation
    {
        #region Анимации цвета
        static public void CreateColorAnimation(Controller controller, Brush fromValue, Color endValue, double duration)
        {
            SolidColorBrush brush = new SolidColorBrush(((SolidColorBrush)fromValue).Color);
            controller.FillColor = brush;

            ColorAnimation colorAnimation = new ColorAnimation();
            colorAnimation.To = endValue;
            colorAnimation.Duration = TimeSpan.FromSeconds(duration);
            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        static public void CreateInfiniteColorAnimation(Controller controller, Color fromValue, Color endValue, double duration)
        {
            SolidColorBrush brush = new SolidColorBrush(fromValue);
            controller.FillColor = brush;

            ColorAnimation colorAnimation = new ColorAnimation();
            colorAnimation.From = fromValue;
            colorAnimation.To = endValue;
            colorAnimation.AutoReverse = true;
            colorAnimation.RepeatBehavior = RepeatBehavior.Forever;
            colorAnimation.Duration = TimeSpan.FromSeconds(duration);
            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }
        #endregion



        #region Анимации формы
        static public void CreateMarkerAnimation(Rectangle marker, double fromValue, double endValue, double duration)
        {
            DoubleAnimation markerAnimation = new DoubleAnimation();
            markerAnimation.From = fromValue;
            markerAnimation.To = endValue;
            markerAnimation.AutoReverse = true;
            markerAnimation.RepeatBehavior = RepeatBehavior.Forever;
            markerAnimation.Duration = TimeSpan.FromSeconds(duration);
            marker.BeginAnimation(Rectangle.StrokeThicknessProperty, markerAnimation);
        }

        static public void CreateScaleAnimation(ScaleTransform mapScaleTransform, double endValue, double duration)
        {
            DoubleAnimation scaleAnimation = new DoubleAnimation();
            scaleAnimation.To = endValue;
            scaleAnimation.Duration = TimeSpan.FromSeconds(duration);
            mapScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);

            scaleAnimation.To = endValue;
            scaleAnimation.Duration = TimeSpan.FromSeconds(duration);
            mapScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }
        #endregion



        #region Анимации прозрачности
        static public void CreateOpacityAnimation(TextBlock control, double duration)
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1;
            opacityAnimation.To = 0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(duration);
            control.BeginAnimation(Rectangle.OpacityProperty, opacityAnimation);
        }
        #endregion
    }
}
