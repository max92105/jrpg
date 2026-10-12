using JRPG.Core.Systems;
using System.Windows;
using System.Windows.Controls;

namespace JRPG.UI.GameMenu.Controls
{
    /// <summary>Right side of the game menu: a card for each active party member.</summary>
    public partial class PartySidebar : UserControl
    {
        public PartySidebar()
        {
            InitializeComponent();
        }

        /// <summary>Rebuilds the cards. Call after anything that changes the party (HP, members, order).</summary>
        public void Refresh()
        {
            MembersPanel.Children.Clear();

            foreach (var character in GameSystems.Party.ActivePartyOrder)
            {
                MembersPanel.Children.Add(new PartyMemberCard(character)
                {
                    Margin = new Thickness(0, 0, 0, 12)
                });
            }
        }
    }
}
