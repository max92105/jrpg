using JRPG.Core.Battles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace JRPG.UI.Battle.Controls
{
    /// <summary>
    /// End of a won battle: the EXP earned, then each member's card with its EXP bar filling up.
    /// Click once to skip the animation, click again to continue.
    /// </summary>
    public partial class VictoryScreen : UserControl
    {
        private static readonly Duration TitleDuration = TimeSpan.FromMilliseconds(450);
        private static readonly TimeSpan RewardDelay = TimeSpan.FromMilliseconds(250);
        private static readonly TimeSpan FirstCardDelay = TimeSpan.FromMilliseconds(450);
        private static readonly TimeSpan CardStagger = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan PauseBeforeExp = TimeSpan.FromMilliseconds(500);
        private static readonly Thickness CardMargin = new Thickness(0, 0, 60, 44);

        /// <summary>The player is done with the screen.</summary>
        public event Action Continued;

        private readonly List<ExpGainRow> cards = new List<ExpGainRow>();
        private bool isPlaying;

        public VictoryScreen()
        {
            InitializeComponent();
        }

        public async void Show(int totalExp, IReadOnlyList<ExpGain> gains)
        {
            Visibility = Visibility.Visible;
            ExpValueText.Text = totalExp.ToString("N0");

            RowsPanel.Children.Clear();
            cards.Clear();
            foreach (var gain in gains)
            {
                var card = new ExpGainRow(gain) { Margin = CardMargin };
                cards.Add(card);
                RowsPanel.Children.Add(card);
            }

            isPlaying = true;

            // Title slides in, then the reward bar, then the cards one after another
            AnimateTitle();
            FadeIn(ExpRewardBar, RewardDelay);

            for (int i = 0; i < cards.Count; i++)
                cards[i].Appear(FirstCardDelay + TimeSpan.FromTicks(CardStagger.Ticks * i));

            // Then all the EXP bars fill together
            await Task.Delay(FirstCardDelay + TimeSpan.FromTicks(CardStagger.Ticks * cards.Count) + PauseBeforeExp);
            await Task.WhenAll(cards.Select(card => card.PlayAsync()));

            FinishPlaying();
        }

        private void AnimateTitle()
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            TitleSlide.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(-60, 0, TitleDuration) { EasingFunction = ease });
            TitleText.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TitleDuration));
        }

        private static void FadeIn(UIElement element, TimeSpan delay)
        {
            element.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300)) { BeginTime = delay });
        }

        /// <summary>Ends the animation (skipped or finished) and shows "Next".</summary>
        private void FinishPlaying()
        {
            if (!isPlaying)
                return;
            isPlaying = false;

            // Everything to its final state
            TitleSlide.BeginAnimation(TranslateTransform.XProperty, null);
            TitleText.BeginAnimation(OpacityProperty, null);
            ExpRewardBar.BeginAnimation(OpacityProperty, null);
            ExpRewardBar.Opacity = 1;

            foreach (var card in cards)
                card.Skip();

            FadeIn(ContinueHint, TimeSpan.Zero);
        }

        private void VictoryScreen_Click(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (isPlaying)
                FinishPlaying();
            else
                Continued?.Invoke();
        }
    }
}
