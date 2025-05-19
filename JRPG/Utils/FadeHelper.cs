using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace JRPG.Core.Utils
{
    public static class FadeHelper
    {
        public static async Task FadeToAsync(FrameworkElement fadeRect, double toOpacity, double durationSec)
        {
            var tcs = new TaskCompletionSource<bool>();

            fadeRect.Visibility = Visibility.Visible;
            fadeRect.Width = Application.Current.MainWindow.ActualWidth;
            fadeRect.Height = Application.Current.MainWindow.ActualHeight;

            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                To = toOpacity,
                Duration = TimeSpan.FromSeconds(durationSec),
                FillBehavior = FillBehavior.Stop
            };

            Storyboard.SetTarget(animation, fadeRect);
            Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(animation);

            storyboard.Completed += (s, e) =>
            {
                fadeRect.Opacity = toOpacity;
                if (toOpacity == 0)
                    fadeRect.Visibility = Visibility.Collapsed;

                tcs.SetResult(true);
            };

            storyboard.Begin();
            await tcs.Task;
        }

        public static Task FadeToBlackAsync(FrameworkElement fadeRect, double durationSec = 0.5)
            => FadeToAsync(fadeRect, 1, durationSec);

        public static Task FadeFromBlackAsync(FrameworkElement fadeRect, double durationSec = 0.5)
            => FadeToAsync(fadeRect, 0, durationSec);
    }
}