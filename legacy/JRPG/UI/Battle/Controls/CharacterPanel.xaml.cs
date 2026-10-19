using JRPG.Core.Battles.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace JRPG.UI.Battle.Controls
{
    /// <summary>A party member's name, HP and MP during battle.</summary>
    public partial class CharacterPanel : UserControl
    {
        private const double ActiveSlide = -36;
        private const double KnockedOutOpacity = 0.55;
        private static readonly Duration BarDuration = TimeSpan.FromMilliseconds(300);
        private static readonly Duration SlideDuration = TimeSpan.FromMilliseconds(250);

        private bool isActive;

        public CharacterPanel()
        {
            InitializeComponent();
        }

        /// <param name="isActive">This member is acting now: the panel slides out and its diamond lights up.</param>
        public void Show(Character character, bool isActive)
        {
            NameText.Text = character.Name;
            Portrait.Show(character);

            ShowStat(HPBar, HPText, MaxHPText, character.HP, character.MaxHP);
            ShowStat(MPBar, MPText, MaxMPText, character.MP, character.MaxMP);

            // Knocked out: red 0 and the whole panel faded
            HPText.Foreground = (Brush)FindResource(character.IsAlive ? "Brush.Text.Primary" : "Brush.Danger");
            Opacity = character.IsAlive ? 1.0 : KnockedOutOpacity;

            SetActive(isActive);
        }

        private static void ShowStat(ProgressBar bar, TextBlock valueText, TextBlock maxText, int value, int max)
        {
            valueText.Text = value.ToString();
            maxText.Text = $"/{max}";

            bar.Maximum = Math.Max(1, max);
            bar.BeginAnimation(ProgressBar.ValueProperty, new DoubleAnimation(Math.Min(value, max), BarDuration)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });
        }

        private void SetActive(bool active)
        {
            Portrait.IsCurrent = active;

            if (isActive == active)
                return;

            isActive = active;
            PanelSlide.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(active ? ActiveSlide : 0, SlideDuration)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });
        }
    }
}
