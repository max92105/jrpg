using JRPG.Core.Battles.Models;
using JRPG.Core.System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JRPG.UI.GameMenu
{
    public partial class EquipmentSelectionView : UserControl
    {
        private Character character;
        private EquipmentManager equipmentManager;
        private PartyManager partyManager;
        private bool isWeapon;

        public EquipmentSelectionView(Character character, EquipmentManager equipmentManager, PartyManager partyManager, bool isWeapon)
        {
            InitializeComponent();
            this.character = character;
            this.equipmentManager = equipmentManager;
            this.partyManager = partyManager;
            this.isWeapon = isWeapon;

            TitleText.Text = $"Select {(isWeapon ? "Weapon" : "Armor")} for {character.Name}";
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
                    equipmentManager.UnequipWeapon(character);
                else
                    equipmentManager.UnequipArmor(character);
                ReturnToEquipmentView();
            };
            ItemListPanel.Children.Add(unequip);

            List<string> items = isWeapon
                ? equipmentManager.GetAvailableWeaponsFor(character.Class)
                : equipmentManager.GetAvailableArmorFor(character.Class);

            foreach (var item in items)
            {
                int currentStat = isWeapon ? character.GetTotalAttack() : character.GetTotalDefense();
                int newStat = isWeapon ? character.BaseAttack + equipmentManager.GetWeaponAttackBonus(item)
                                       : character.BaseDefense + equipmentManager.GetArmorDefenseBonus(item);

                int stock = isWeapon ? equipmentManager.GetWeaponStock(item) : equipmentManager.GetArmorStock(item);
                if (stock <= 0) continue;

                int diff = newStat - currentStat;
                string statChange = diff > 0 ? $"▲ (+{diff})"
                                    : diff < 0 ? $"▼ ({diff})"
                                    : "(no change)";

                var textBlock = new TextBlock
                {
                    Text = $"{item} ({(isWeapon ? "ATK" : "DEF")}: {newStat}) {statChange}   (x{stock})",
                    Cursor = Cursors.Hand,
                    Foreground = System.Windows.Media.Brushes.White,
                    Margin = new Thickness(0, 0, 0, 10),
                    FontSize = 18
                };

                textBlock.MouseLeftButtonUp += (s, e) =>
                {
                    if (isWeapon)
                        equipmentManager.EquipWeapon(character, item);
                    else
                        equipmentManager.EquipArmor(character, item);
                    ReturnToEquipmentView();
                };

                ItemListPanel.Children.Add(textBlock);
            }
        }

        private void ReturnToEquipmentView()
        {
            var view = new EquipmentView(
                partyManager,
                equipmentManager
            );

            ((MainWindow)Application.Current.MainWindow).GameMenuControl.ShowContent(view);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ReturnToEquipmentView();
        }
    }
}
