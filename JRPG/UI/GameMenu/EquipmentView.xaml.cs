using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Systems.Equipments.Models;
using JRPG.Utils;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace JRPG.UI.GameMenu
{
    public partial class EquipmentView : UserControl, IFilterable<Character>
    {
        private Character selectedCharacter;

        public EquipmentView(Character selectedCharacter = null)
        {
            InitializeComponent();

            WeaponNameBlock.MouseLeftButtonUp += WeaponRow_Click;
            ArmorNameBlock.MouseLeftButtonUp += ArmorRow_Click;

            if (selectedCharacter != null)
            {
                ApplyFilter(selectedCharacter);           
            }
            else
            {
                // Default selection
                var firstCharacter = GameSystems.Party.AllCharacters.FirstOrDefault();
                if (firstCharacter != null)
                {
                    ApplyFilter(firstCharacter);
                }
            }
        }

        public void ApplyFilter(Character character)
        {
            if (character == null) return;

            selectedCharacter = character;
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (selectedCharacter == null) return;

            CharacterNameBlock.Text = selectedCharacter.Name;
            WeaponNameBlock.Text = selectedCharacter.EquippedWeapon?.Name ?? "None";
            ArmorNameBlock.Text = selectedCharacter.EquippedArmor?.Name ?? "None";

            cStatsPanel.Load(selectedCharacter);
        }

        private TextBlock CreateStatText(string label, int value)
        {
            return new TextBlock
            {
                Text = $"{label}: {value}",
                FontSize = 16,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(2)
            };
        }

        public void PreviewStats(Equipment equipment, bool isWeapon)
        {
            cStatsPanel.PreviewStatChange(selectedCharacter, equipment, isWeapon);
        }

        private void WeaponRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (selectedCharacter == null) return;

            var view = new EquipmentSelectionView(selectedCharacter, isWeapon: true);
            ((MainWindow)Application.Current.MainWindow).GameMenuControl.ShowSecondary(view);
        }

        private void ArmorRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (selectedCharacter == null) return;

            var view = new EquipmentSelectionView(selectedCharacter, isWeapon: false);
            ((MainWindow)Application.Current.MainWindow).GameMenuControl.ShowSecondary(view);
        }
    }
}