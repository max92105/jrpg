using JRPG.Core.Battles.Enum;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace JRPG.UI.Battle.Controls
{
    /// <summary>
    /// Interaction logic for FloatingText.xaml
    /// </summary>
    public partial class FloatingText : UserControl
    {
        private TranslateTransform transform = new TranslateTransform();

        public FloatingText(string text, EffectType type)
        {
            InitializeComponent();
            TextLabel.Text = text;

            // Set color              
            switch (type)
            {
                case EffectType.Damage:
                    TextLabel.Foreground = Brushes.Red;
                    break;
                case EffectType.Heal:
                    TextLabel.Foreground = Brushes.LightGreen;
                    break;
                case EffectType.ManaRecovery:
                    TextLabel.Foreground = Brushes.LightBlue;
                    break;
                default:
                    TextLabel.Foreground = Brushes.White;
                    break;
            };

            RenderTransform = transform;
            RenderTransformOrigin = new Point(0.5, 0.5);

            Loaded += (s, e) => StartAnimation();
        }

        private void StartAnimation()
        {
            var moveUp = new DoubleAnimation
            {
                From = 0,
                To = -50,
                Duration = TimeSpan.FromSeconds(2),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(2)
            };

            transform.BeginAnimation(TranslateTransform.YProperty, moveUp);
            TextLabel.BeginAnimation(OpacityProperty, fadeOut);

            fadeOut.Completed += (s, e) =>
            {
                var parent = Parent as Panel;
                parent?.Children.Remove(this);
            };
        }
    }
}
