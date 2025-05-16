using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Systems.Equipments.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JRPG.UI.GameMenu
{
    public partial class EquipmentSelectionView : UserControl
    {
        private Character character;
        private bool isWeapon;

        public EquipmentSelectionView(Character character, bool isWeapon)
        {
            InitializeComponent();
            this.character = character;
            this.isWeapon = isWeapon;

            HeaderText.Text = $"Select {(isWeapon ? "Weapon" : "Armor")} for {character.Name}";
            BuildItemList();
        }

        private void BuildItemList()
        {
            ItemListPanel.Children.Clear();

            var unequip = new TextBlock
            {
                Text = "None (Unequip)",
                Cursor = Cursors.Hand,
                Foreground = System.Windows.Media.Brushes.LightBlue,
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 18
            };
            unequip.MouseLeftButtonUp += (s, e) =>
            {
                if (isWeapon)
                    GameSystems.Equipment.UnequipWeapon(character);
                else
                    GameSystems.Equipment.UnequipArmor(character);
                ReturnToEquipmentView();
            };
            ItemListPanel.Children.Add(unequip);

            var entries = GameSystems.Equipment.EquipmentInventory
                .Where(e =>
                    e.Quantity > 0 &&
                    e.Equipment is Weapon == isWeapon &&
                    e.Equipment.AllowedClasses.Contains(character.Class))
                .ToList();

            foreach (var entry in entries)
            {
                var equipment = entry.Equipment;
                int currentStat = isWeapon ? character.GetTotalAttack() : character.GetTotalDefense();
                int newStat = isWeapon
                    ? character.BaseAttack + ((Weapon)equipment).AttackBonus
                    : character.BaseDefense + ((Armor)equipment).DefenseBonus;

                int diff = newStat - currentStat;
                string statChange = diff > 0 ? $"▲ (+{diff})"
                                    : diff < 0 ? $"▼ ({diff})"
                                    : "(no change)";

                var textBlock = new TextBlock
                {
                    Text = $"{equipment.Name} ({(isWeapon ? "ATK" : "DEF")}: {newStat}) {statChange}   (x{entry.Quantity})",
                    Cursor = Cursors.Hand,
                    Foreground = System.Windows.Media.Brushes.White,
                    Margin = new Thickness(0, 0, 0, 10),
                    FontSize = 18
                };

                textBlock.MouseLeftButtonUp += (s, e) =>
                {
                    if (isWeapon)
                        GameSystems.Equipment.Equip(character, equipment);
                    else
                        GameSystems.Equipment.Equip(character, equipment);
                    ReturnToEquipmentView();
                };

                ItemListPanel.Children.Add(textBlock);
            }
        }

        private void ReturnToEquipmentView()
        {
            var view = new EquipmentView();
            
            ((MainWindow)Application.Current.MainWindow).GameMenuControl.ShowContent(view);
            ((MainWindow)Application.Current.MainWindow).GameMenuControl.HideSecondary();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ReturnToEquipmentView();
        }
    }
}
