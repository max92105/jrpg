using JRPG.Core.Battles.Models;
using JRPG.Core.Utils;
using JRPG.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JRPG.UI.GameMenu.Controls
{
    public partial class CharacterPortrait : UserControl
    {
        public CharacterPortrait()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        public static readonly DependencyProperty CharacterProperty =
            DependencyProperty.Register(nameof(Character), typeof(Character), typeof(CharacterPortrait),
                new PropertyMetadata(null, OnCharacterChanged));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(CharacterPortrait),
                new PropertyMetadata(false, OnSelectionChanged));

        public static readonly DependencyProperty IsEnabledTargetProperty =
            DependencyProperty.Register(nameof(IsEnabledTarget), typeof(bool), typeof(CharacterPortrait),
                new PropertyMetadata(true, OnEnabledChanged));

        public static readonly DependencyProperty ShowStatsProperty =
    DependencyProperty.Register(nameof(ShowStats), typeof(bool), typeof(CharacterPortrait),
        new PropertyMetadata(true, OnShowStatsChanged));

        public Character Character
        {
            get => (Character)GetValue(CharacterProperty);
            set => SetValue(CharacterProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public bool IsEnabledTarget
        {
            get => (bool)GetValue(IsEnabledTargetProperty);
            set => SetValue(IsEnabledTargetProperty, value);
        }

        public bool ShowStats
        {
            get => (bool)GetValue(ShowStatsProperty);
            set => SetValue(ShowStatsProperty, value);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisuals();
        }

        private static void OnCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CharacterPortrait control)
                control.UpdateVisuals();
        }

        private static void OnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CharacterPortrait control)
            {
                if (control.IsSelected)
                {
                    control.PortraitImage.Effect = new DropShadowEffect
                    {
                        Color = Colors.White,
                        BlurRadius = 20,
                        ShadowDepth = 0,
                        Opacity = 0.6
                    };
                }
                else
                {
                    control.PortraitImage.Effect = null;
                }
            }
        }

        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CharacterPortrait control)
                control.UpdateVisuals();
        }

        private static void OnShowStatsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CharacterPortrait control)
                control.UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (Character == null)
                return;

            PortraitImage.Source = ImageLoader.LoadPortrait(Character.PortraitFile);

            HPBar.Visibility = ShowStats ? Visibility.Visible : Visibility.Collapsed;
            MPBar.Visibility = ShowStats ? Visibility.Visible : Visibility.Collapsed;

            if (ShowStats)
            {
                HPBar.Maximum = Character.MaxHP;
                HPBar.Value = Character.HP;

                MPBar.Maximum = Character.MaxMP;
                MPBar.Value = Character.MP;
            }
            // Fainting effect (grayscale or opacity)
            PortraitImage.Opacity = Character.IsAlive ? 1.0 : 0.4;
        }
    }
}
