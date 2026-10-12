using JRPG.Core.Battles.Enums;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace JRPG.UI.Battle.Controls
{
    /// <summary>A number that pops over a target (white = damage, green = heal, blue = MP), rises and fades.</summary>
    public partial class FloatingText : UserControl
    {
        public const double HalfWidth = 80; // so callers can center it on a point

        private const double RiseDistance = -50;
        private static readonly Duration PopDuration = TimeSpan.FromMilliseconds(180);
        private static readonly Duration LifeDuration = TimeSpan.FromSeconds(1.6);

        private readonly TranslateTransform rise = new TranslateTransform();
        private readonly ScaleTransform pop = new ScaleTransform(0.6, 0.6);

        public FloatingText(string text, EffectType type)
        {
            InitializeComponent();
            TextLabel.Text = text;
            TextLabel.Foreground = BrushFor(type);

            var transforms = new TransformGroup();
            transforms.Children.Add(pop);
            transforms.Children.Add(rise);
            RenderTransform = transforms;
            RenderTransformOrigin = new Point(0.5, 0.5);

            Loaded += (s, e) => StartAnimation();
        }

        private Brush BrushFor(EffectType type)
        {
            switch (type)
            {
                case EffectType.Heal: return (Brush)FindResource("Brush.Bar.HP");
                case EffectType.ManaRecovery: return (Brush)FindResource("Brush.Bar.MP");
                default: return (Brush)FindResource("Brush.Text.Primary");
            }
        }

        private void StartAnimation()
        {
            // Pop: grows a bit past full size, then settles
            var popAnimation = new DoubleAnimation(1.0, PopDuration)
            {
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.6 }
            };
            pop.BeginAnimation(ScaleTransform.ScaleXProperty, popAnimation);
            pop.BeginAnimation(ScaleTransform.ScaleYProperty, popAnimation);

            rise.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0, RiseDistance, LifeDuration)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });

            // Stay solid for the first half, then fade
            var fade = new DoubleAnimationUsingKeyFrames();
            fade.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.5)));
            fade.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1.0)));
            fade.Duration = LifeDuration;
            fade.Completed += (s, e) => (Parent as Panel)?.Children.Remove(this);

            BeginAnimation(OpacityProperty, fade);
        }
    }
}
