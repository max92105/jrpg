using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Systems.Equipments.Models;
using JRPG.Core.Systems.Inventories.Models;
using JRPG.UI.GameMenu.Controls;
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
        private bool isWaitingForUnequipSelection = false;
        private Equipment hoveredEquipment = null;
        private Equipment pendingEquip = null;

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
            EquipOnPortrait.Children.Clear();

            // "None" unequip option
            EquipmentListPanel.Children.Add(CreateEquipmentRow(
                label: "None",
                quantity: "-",
                foreground: Brushes.LightBlue,
                onClick: () =>
                {
                    GameSystems.Equipment.Unequip(character, null);
                    ReturnToEquipmentView();
                }));

            // All valid equipment
            var entries = GameSystems.Inventory.Items
                .Where(e =>
                    e.Item is Equipment eq &&
                    eq.AllowedClasses.Contains(character.Class) &&
                    ((isWeapon && eq is Weapon) || (!isWeapon && eq is Armor)))
                .ToList();

            foreach (var entry in entries)
            {
                var equipment = (Equipment)entry.Item;
                int available = entry.AvailableStock;
                bool alreadyEquipped = entry.EquippedTo.Contains(character);

                string label = equipment.Name;
                string quantityLabel = $"x{entry.Quantity}";

                // Row is grayed out if fully equipped and not equipped by this character
                Brush rowColor = (available == 0 && !alreadyEquipped) ? Brushes.Gray : Brushes.White;

                EquipmentListPanel.Children.Add(CreateEquipmentRow(
                    label,
                    quantityLabel,
                    rowColor,
                    () =>
                    {
                        if (isWaitingForUnequipSelection) return;

                        if (available > 0 || alreadyEquipped)
                        {
                            GameSystems.Equipment.Equip(character, equipment);
                            ReturnToEquipmentView();
                        }
                        else
                        {
                            pendingEquip = equipment;
                            isWaitingForUnequipSelection = true;
                            HighlightSelectionLock(true);
                            ItemDescriptionBlock.Text = "Select a character below to unequip from.";
                            ShowEquippedCharacters(equipment, allowClick: true);
                        }
                    },
                    previewEquipment: equipment
                ));
            }
        }

        private Grid CreateEquipmentRow(string label, string quantity, Brush foreground, Action onClick, Equipment previewEquipment = null)
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

            // 👇 Add hover preview if previewEquipment is passed
            if (previewEquipment != null)
            {
                row.MouseEnter += (s, e) =>
                {
                    if (isWaitingForUnequipSelection)
                        return;

                    hoveredEquipment = previewEquipment;

                    // 📝 Update item description
                    ItemDescriptionBlock.Text = previewEquipment.Description;

                    var menuControl = (MainWindow)Application.Current.MainWindow;
                    var equipmentView = menuControl.GameMenuControl.GameMenuContent.Content as EquipmentView;
                    equipmentView?.PreviewStats(previewEquipment, isWeapon);

                    ShowEquippedCharacters(previewEquipment, allowClick: false);
                };

                row.MouseLeave += (s, e) =>
                {
                    if (!isWaitingForUnequipSelection && hoveredEquipment == previewEquipment)
                    {
                        hoveredEquipment = null;

                        var menuControl = (MainWindow)Application.Current.MainWindow;
                        var equipmentView = menuControl.GameMenuControl.GameMenuContent.Content as EquipmentView;
                        equipmentView?.ApplyFilter(character);

                        EquipOnPortrait.Children.Clear();
                    }
                };
            }

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

        private void ShowEquippedCharacters(Equipment equipment, bool allowClick)
        {
            EquipOnPortrait.Children.Clear();

            var entry = GameSystems.Inventory.Items
                .FirstOrDefault(e => e.Item == equipment);

            if (entry == null || entry.EquippedTo.Count == 0) return;

            foreach (var user in entry.EquippedTo)
            {
                var portrait = new CharacterPortrait
                {
                    Character = user,
                    IsSelected = false,
                    ShowStats = false,
                    IsEnabledTarget = allowClick,
                    Margin = new Thickness(4),
                    Cursor = allowClick ? Cursors.Hand : Cursors.Arrow,
                    Opacity = allowClick ? 1.0 : 0.5 // faded if not interactive
                };

                if (allowClick)
                {
                    portrait.MouseLeftButtonUp += (s, e) =>
                    {
                        GameSystems.Equipment.Unequip(user, pendingEquip);
                        GameSystems.Equipment.Equip(character, pendingEquip);
                        ReturnToEquipmentView();
                    };
                }

                EquipOnPortrait.Children.Add(portrait);
            }
        }

        private void HighlightSelectionLock(bool enabled)
        {
            EquipmentListPanel.Opacity = enabled ? 0.4 : 1.0;
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