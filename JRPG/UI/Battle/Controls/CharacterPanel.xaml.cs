using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace JRPG.UI.Battle.Controls
{
    /// <summary>
    /// Interaction logic for CharacterPanel.xaml
    /// </summary>
    public partial class CharacterPanel : UserControl
    {
        private int hp, maxHp, mp, maxMp;

        public CharacterPanel()
        {
            InitializeComponent();
        }

        public bool IsActive
        {
            set
            {
                var targetOffset = value ? -100 : 0;
                var duration = TimeSpan.FromMilliseconds(300);

                var slide = new DoubleAnimation
                {
                    To = targetOffset,
                    Duration = duration,
                    EasingFunction = new QuadraticEase
                    {
                        EasingMode = value ? EasingMode.EaseOut : EasingMode.EaseIn
                    }
                };

                PanelTransform.BeginAnimation(TranslateTransform.XProperty, slide);
            }
        }

        public string CharacterName
        {
            get => NameText.Text;
            set => NameText.Text = value;
        }

        public void SetPortrait(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                PortraitImage.Source = new BitmapImage(new Uri(imagePath));
                PortraitImage.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                PortraitImage.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public int CurrentHP
        {
            get => hp;
            set { hp = Math.Max(0, value); UpdateHP(); }
        }

        public int MaxHP
        {
            get => maxHp;
            set { maxHp = Math.Max(1, value); UpdateHP(); }
        }

        public int CurrentMP
        {
            get => mp;
            set { mp = Math.Max(0, value); UpdateMP(); }
        }

        public int MaxMP
        {
            get => maxMp;
            set { maxMp = Math.Max(1, value); UpdateMP(); }
        }

        private void UpdateHP()
        {
            HPBar.Maximum = maxHp;

            var animation = new DoubleAnimation
            {
                To = Math.Min(hp, maxHp),
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            HPBar.BeginAnimation(ProgressBar.ValueProperty, animation);

            HPText.Text = $"{hp} / {maxHp}";
        }

        private void UpdateMP()
        {
            MPBar.Maximum = maxMp;

            var animation = new DoubleAnimation
            {
                To = Math.Min(mp, maxMp),
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            MPBar.BeginAnimation(ProgressBar.ValueProperty, animation);

            MPText.Text = $"{mp} / {maxMp}";
        }
    }
}
