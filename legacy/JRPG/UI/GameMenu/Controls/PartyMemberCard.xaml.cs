using JRPG.Core.Battles.Models;
using JRPG.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JRPG.UI.GameMenu.Controls
{
    /// <summary>One party member: portrait, name, level, class, HP and MP.</summary>
    public partial class PartyMemberCard : UserControl
    {
        private const double FaintedOpacity = 0.4;

        private bool isInteractive;

        public PartyMemberCard(Character character)
        {
            InitializeComponent();
            Character = character;

            PortraitImage.Source = ImageLoader.LoadPortrait(character.PortraitFile);
            PortraitImage.Opacity = character.IsAlive ? 1.0 : FaintedOpacity;

            NameText.Text = character.Name;
            LevelText.Text = $"Lv {character.Level}";
            ClassText.Text = character.Class;
            LeaderBadge.Visibility = character.IsMainCharacter ? Visibility.Visible : Visibility.Collapsed;

            HPBar.Maximum = character.MaxHP;
            HPBar.Value = character.HP;
            HPText.Text = $"{character.HP} / {character.MaxHP}";

            MPBar.Maximum = character.MaxMP;
            MPBar.Value = character.MP;
            MPText.Text = $"{character.MP} / {character.MaxMP}";

            MouseEnter += (s, e) => HoverHighlight.Opacity = isInteractive ? 1 : 0;
            MouseLeave += (s, e) => HoverHighlight.Opacity = 0;
        }

        public Character Character { get; }

        /// <summary>Lights up on hover and shows the hand cursor. Clicks are handled by whoever creates the card.</summary>
        public bool IsInteractive
        {
            get => isInteractive;
            set
            {
                isInteractive = value;
                Cursor = value ? Cursors.Hand : null;
            }
        }
    }
}
