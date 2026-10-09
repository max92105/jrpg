using JRPG.Utils;
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
        private bool isActive;

        public CharacterPanel()
        {
            InitializeComponent();

            isActive = false;
        }

        public bool IsActive
        {
            set
            {
                if (isActive == value)
                    return;

                isActive = value;

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
                PortraitImage.Source = ImageLoader.Load(imagePath, 256);
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
            set
            {
                var clamped = Math.Max(0, value);
                if (clamped != hp)
                {
                    hp = clamped;
                    UpdateHP();
                }
            }
        }

        public int MaxHP
        {
            get => maxHp;
            set
            {
                var clamped = Math.Max(1, value);
                if (clamped != maxHp)
                {
                    maxHp = clamped;
                    UpdateHP();
                }
            }
        }

        public int CurrentMP
        {
            get => mp;
            set
            {
                var clamped = Math.Max(0, value);
                if (clamped != mp)
                {
                    mp = clamped;
                    UpdateMP();
                }
            }
        }

        public int MaxMP
        {
            get => maxMp;
            set
            {
                var clamped = Math.Max(1, value);
                if (clamped != maxMp)
                {
                    maxMp = clamped;
                    UpdateMP();
                }
            }
        }

        private void UpdateHP()
        {
            HPBar.Maximum = maxHp;

            double currentValue = HPBar.Value;
            double targetValue = Math.Min(hp, maxHp);

            // Prevent animation if value is unchanged
            if (Math.Abs(currentValue - targetValue) < 0.1)
                return;

            HPBar.BeginAnimation(ProgressBar.ValueProperty, null); // Cancel ongoing animation

            var animation = new DoubleAnimation
            {
                From = currentValue,
                To = targetValue,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            HPBar.BeginAnimation(ProgressBar.ValueProperty, animation);

            HPText.Text = $"{hp} / {maxHp}";
        }

        private void UpdateMP()
        {
            MPBar.Maximum = maxMp;

            double currentValue = MPBar.Value;
            double targetValue = Math.Min(mp, maxMp);

            if (Math.Abs(currentValue - targetValue) < 0.1)
                return;

            MPBar.BeginAnimation(ProgressBar.ValueProperty, null);

            var animation = new DoubleAnimation
            {
                From = currentValue,
                To = targetValue,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            MPBar.BeginAnimation(ProgressBar.ValueProperty, animation);

            MPText.Text = $"{mp} / {maxMp}";
        }
    }
}
