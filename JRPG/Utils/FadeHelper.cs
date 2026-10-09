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

            // Ensure it's visible and fully stretched
            fadeRect.Visibility = Visibility.Visible;
            fadeRect.Opacity = fadeRect.Opacity;

            // Let one render pass happen before starting the animation
            // (a Task.Delay(1) here used to add ~15ms of lag to every fade)
            await Application.Current.Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Render);

            var animation = new DoubleAnimation
            {
                To = toOpacity,
                Duration = TimeSpan.FromSeconds(durationSec),
                FillBehavior = FillBehavior.HoldEnd
            };

            Storyboard.SetTarget(animation, fadeRect);
            Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.OpacityProperty));

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);

            storyboard.Completed += (s, e) =>
            {
                fadeRect.Opacity = toOpacity;
                if (toOpacity == 0)
                    fadeRect.Visibility = Visibility.Collapsed;

                tcs.TrySetResult(true);
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