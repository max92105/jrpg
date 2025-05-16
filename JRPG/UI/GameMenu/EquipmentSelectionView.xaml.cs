using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Systems.Equipments.Models;
using JRPG.Core.Systems.Inventories.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace JRPG.UI.GameMenu
{
    public partial class EquipmentSelectionView : UserControl
    {
        private readonly Character character;
        private readonly bool isWeapon;

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
            EquipmentListPanel.Children.Clear();

            // Add "None" unequip option
            EquipmentListPanel.Children.Add(CreateEquipmentRow(
                label: "None",
                quantity: "-",
                foreground: Brushes.LightBlue,
                onClick: () =>
                {
                    if (isWeapon)
                        GameSystems.Equipment.UnequipWeapon(character);
                    else
                        GameSystems.Equipment.UnequipArmor(character);
                    ReturnToEquipmentView();
                }));

            // Filter inventory for valid equipment
            var entries = GameSystems.Inventory.Items
                .Where(e =>
                    e.Quantity > 0 &&
                    e.Item is Equipment eq &&
                    eq.AllowedClasses.Contains(character.Class) &&
                    ((isWeapon && eq is Weapon) || (!isWeapon && eq is Armor)))
                .ToList();

            foreach (var entry in entries)
            {
                var equipment = (Equipment)entry.Item;

                int baseStat = isWeapon ? character.BaseAttack : character.BaseDefense;
                int newBonus = isWeapon
                    ? ((Weapon)equipment).AttackBonus
                    : ((Armor)equipment).DefenseBonus;

                int newStat = baseStat + newBonus;
                int currentStat = isWeapon ? character.GetTotalAttack() : character.GetTotalDefense();
                int diff = newStat - currentStat;

                string statChange = diff > 0 ? $"▲ (+{diff})"
                                   : diff < 0 ? $"▼ ({diff})"
                                   : "(no change)";

                string label = $"{equipment.Name} ({(isWeapon ? "ATK" : "DEF")}: {newStat}) {statChange}";

                EquipmentListPanel.Children.Add(CreateEquipmentRow(
                    label,
                    $"x{entry.Quantity}",
                    Brushes.White,
                    () =>
                    {
                        GameSystems.Equipment.Equip(character, equipment);
                        ReturnToEquipmentView();
                    }));
            }
        }

        private Grid CreateEquipmentRow(string label, string quantity, Brush foreground, Action onClick)
        {
            var row = new Grid
            {
                Style = (Style)FindResource("InventoryItemRowStyle")
            };

            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

            var nameBlock = new TextBlock
            {
                Text = label,
                FontSize = 16,
                Foreground = foreground,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nameBlock, 0);
            row.Children.Add(nameBlock);

            var quantityBlock = new TextBlock
            {
                Text = quantity,
                FontSize = 16,
                Foreground = foreground,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 10, 0)
            };
            Grid.SetColumn(quantityBlock, 1);
            row.Children.Add(quantityBlock);

            row.MouseLeftButtonUp += (s, e) => onClick();

            return row;
        }

        private TextBlock CreateOptionText(string text, Brush color, Action onClick)
        {
            var block = new TextBlock
            {
                Text = text,
                Cursor = Cursors.Hand,
                Foreground = color,
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 18
            };

            block.MouseLeftButtonUp += (s, e) => onClick?.Invoke();
            return block;
        }

        private void ReturnToEquipmentView()
        {
            ((MainWindow)Application.Current.MainWindow).GameMenuControl.ShowContent(new EquipmentView(character));
            ((MainWindow)Application.Current.MainWindow).GameMenuControl.HideSecondary();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ReturnToEquipmentView();
        }
    }
}