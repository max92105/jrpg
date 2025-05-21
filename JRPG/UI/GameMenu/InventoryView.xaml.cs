using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JRPG.Core.Battles.Enums;
using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Systems.Inventories.Models;
using JRPG.Core.Utils;
using JRPG.UI.GameMenu.Controls;
using JRPG.Utils;

namespace JRPG.UI.GameMenu
{
    public partial class InventoryView : UserControl, IFilterable<Type>
    {
        private Type selectedFilterType;
        private readonly List<Character> selectedTargets = new List<Character>();

        public InventoryView()
        {
            InitializeComponent();
            Refresh();
        }

        public void ApplyFilter(Type filterValue)
        {
            selectedFilterType = filterValue;
            FilterHeader.Text = filterValue == null ? "All Items" : $"{filterValue.Name}s";
            Refresh();
        }

        private void Refresh()
        {
            ItemListPanel.Children.Clear();
            ItemDescriptionBlock.Text = "Hover an item to see its description.";

            foreach (var entry in GameSystems.Inventory.Items)
            {
                if (selectedFilterType != null && !selectedFilterType.IsInstanceOfType(entry.Item))
                    continue;

                var row = new Grid
                {
                    Style = (Style)FindResource("InventoryItemRowStyle")
                };

                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

                var leftPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var icon = new Image
                {
                    Source = new BitmapImage(new Uri(AssetPathResolver.GetIconsPath(entry.Item.IconFile))),
                    Width = 32,
                    Height = 32,
                    Margin = new Thickness(0, 0, 10, 0)
                };

                var nameText = new TextBlock
                {
                    Text = entry.Item.Name,
                    FontSize = 16,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center
                };

                leftPanel.Children.Add(icon);
                leftPanel.Children.Add(nameText);
                Grid.SetColumn(leftPanel, 0);
                row.Children.Add(leftPanel);

                var quantityText = new TextBlock
                {
                    Text = "x" + entry.Quantity,
                    FontSize = 16,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 0, 10, 0)
                };

                Grid.SetColumn(quantityText, 1);
                row.Children.Add(quantityText);

                row.MouseEnter += (s, e) =>
                {
                    ItemDescriptionBlock.Text = entry.Item.Description;
                };

                if (entry.Item.CanBeUsedFromInventory)
                {
                    row.MouseLeftButtonDown += (s, e) =>
                    {
                        ShowPopup(entry);
                    };
                }

                ItemListPanel.Children.Add(row);
            }
        }

        private void ShowPopup(InventoryItem item)
        {
            GameSystems.Inventory.SelectItem(item);
            selectedTargets.Clear();

            var fullParty = GameSystems.Party.AllCharacters;
            var targetType = item.Item is Consumable consumable ? consumable.TargetType : TargetType.None;

            switch (targetType)
            {
                case TargetType.AllAllies:
                    selectedTargets.AddRange(fullParty.Where(c => c.IsAlive));
                    break;
                case TargetType.AllFaintedAllies:
                    selectedTargets.AddRange(fullParty.Where(c => !c.IsAlive));
                    break;
            }

            PopupQuestion.Text = $"Use {item.Item.Name}";
            PopupIcon.Source = new BitmapImage(new Uri(AssetPathResolver.GetIconsPath(item.Item.IconFile)));

            RefreshPortraitVisuals();

            PopupOverlay.Visibility = Visibility.Visible;
        }

        private void RefreshPortraitVisuals()
        {
            PartyTargetPanel.Children.Clear();

            var selectedItem = GameSystems.Inventory.SelectedItem;
            
            var item = selectedItem?.Item as Consumable;
            if (item == null)
            {
                return;
            }

            var consumable = item;

            var targetType = consumable.TargetType;

            foreach (var character in GameSystems.Party.AllCharacters)
            {
                bool isValid = IsCharacterValidTarget(targetType, character);
                bool isSelected = selectedTargets.Contains(character);

                var portrait = new CharacterPortrait
                {
                    Character = character,
                    IsEnabledTarget = isValid,
                    IsSelected = isSelected,
                    Margin = new Thickness(4),
                    Cursor = isValid ? Cursors.Hand : Cursors.Arrow,
                    IsEnabled = isValid
                };

                bool isSelectionLocked =
                    targetType == TargetType.AllAllies ||
                    targetType == TargetType.AllFaintedAllies;

                if (isValid && !isSelectionLocked)
                {
                    portrait.MouseLeftButtonUp += (s, e) =>
                    {
                        if (!selectedTargets.Contains(character))
                        {
                            if (targetType is TargetType.Ally
                             || targetType is TargetType.SingleFaintedAlly
                             || targetType is TargetType.Self)
                                selectedTargets.Clear();

                            selectedTargets.Add(character);
                        }
                        else
                        {
                            selectedTargets.Remove(character);
                        }

                        RefreshPortraitVisuals();
                    };
                }

                PartyTargetPanel.Children.Add(portrait);
            }
        }

        private bool IsCharacterValidTarget(TargetType type, Character character)
        {
            switch (type)
            {
                case TargetType.Self:
                case TargetType.Ally:
                case TargetType.AllAllies:
                    return character.IsAlive;

                case TargetType.SingleFaintedAlly:
                case TargetType.AllFaintedAllies:
                    return !character.IsAlive;

                default:
                    return false;
            }
        }

        private void PopupYes_Click(object sender, RoutedEventArgs e)
        {
            var targets = selectedTargets.OfType<BattleActor>().ToList();

            if (targets.Count > 0)
            {
                GameSystems.Inventory.UseSelectedItem(null, targets);
            }

            PopupOverlay.Visibility = Visibility.Collapsed;
            Refresh();
        }

        private void PopupNo_Click(object sender, RoutedEventArgs e)
        {
            GameSystems.Inventory.ClearSelection();
            PopupOverlay.Visibility = Visibility.Collapsed;
        }
    }
}
