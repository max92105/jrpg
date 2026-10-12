using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace JRPG.UI.Battle.Controls
{
    /// <summary>Fades a short message in at the top of the battle, holds it, then fades it out.</summary>
    public partial class BattleBanner : UserControl
    {
        private static readonly TimeSpan FadeIn = TimeSpan.FromMilliseconds(150);
        private static readonly TimeSpan FadeOut = TimeSpan.FromMilliseconds(300);
        private static readonly TimeSpan DefaultHold = TimeSpan.FromSeconds(1.2);

        public BattleBanner()
        {
            InitializeComponent();
        }

        public void Show(string message) => Show(message, DefaultHold);

        public void Show(string message, TimeSpan hold)
        {
            MessageText.Text = message;

            var fade = new DoubleAnimationUsingKeyFrames();
            fade.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(FadeIn)));
            fade.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(FadeIn + hold)));
            fade.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(FadeIn + hold + FadeOut)));

            BeginAnimation(OpacityProperty, fade);
        }
    }
}
