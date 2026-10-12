using JRPG.Enums;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace JRPG.UI.Battle.Controls
{
    /// <summary>End of a lost battle: "Defeat", then load a save or go back to the title screen.</summary>
    public partial class DefeatScreen : UserControl
    {
        private static readonly TimeSpan TitleFade = TimeSpan.FromMilliseconds(600);
        private static readonly TimeSpan ChoicesDelay = TimeSpan.FromMilliseconds(700);

        /// <summary>LoadSave or TitleScreen.</summary>
        public event Action<BattleExit> Chosen;

        private bool hasChosen;

        public DefeatScreen()
        {
            InitializeComponent();
        }

        public void Show()
        {
            Visibility = Visibility.Visible;

            // The title fades in slowly, then the choices appear
            TitleText.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TitleFade));
            Choices.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300)) { BeginTime = ChoicesDelay });
        }

        private void LoadSave_Click(object sender, RoutedEventArgs e) => Choose(BattleExit.LoadSave);

        private void TitleScreen_Click(object sender, RoutedEventArgs e) => Choose(BattleExit.TitleScreen);

        private void Choose(BattleExit exit)
        {
            if (hasChosen)
                return;
            hasChosen = true;

            Chosen?.Invoke(exit);
        }
    }
}
