using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using JRPG.Core.Battles.Enums;
using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Systems.Inventories;
using JRPG.Core.Systems.Inventories.Enums;
using JRPG.Core.Systems.Inventories.Models;
using JRPG.Core.Systems.Parties;
using JRPG.Core.Utils;
using JRPG.UI.GameMenu.Controls;
using JRPG.Utils;

namespace JRPG.UI.GameMenu
{
    public partial class InventoryView : UserControl, IFilterable<ItemType?>
    {
        private ItemType? selectedFilterType;

        private readonly List<Character> selectedTargets = new List<Character>();

        public InventoryView()
        {
            InitializeComponent();

            Refresh();
        }

        public void ApplyFilter(ItemType? filterValue)
        {
            selectedFilterType = filterValue;
            FilterHeader.Text = filterValue == null ? "All Items" : $"{filterValue}s";
            Refresh();
        }

        private void Refresh()
        {
            ItemListPanel.Children.Clear();
            ItemDescriptionBlock.Text = "Hover an item to see its description.";

            foreach (var item in GameSystems.Inventory.Items)
            {
                if (selectedFilterType != null && item.Type != selectedFilterType.Value)
                    continue;

                var rowGrid = new Grid
                {
                    Style = (Style)FindResource("InventoryItemRowStyle")
                };

                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

                // Left side (Icon + Name)
                var leftPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var icon = new Image
                {
                    Source = new BitmapImage(new Uri(AssetPathResolver.GetIconsPath(item.IconFile))),
                    Width = 32,
                    Height = 32,
                    Margin = new Thickness(0, 0, 10, 0)
                };

                var nameText = new TextBlock
                {
                    Text = item.Name,
                    FontSize = 16,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center
                };

                leftPanel.Children.Add(icon);
                leftPanel.Children.Add(nameText);
                Grid.SetColumn(leftPanel, 0);
                rowGrid.Children.Add(leftPanel);

                // Right side (Quantity)
                var quantityText = new TextBlock
                {
                    Text = "x" + item.Quantity,
                    FontSize = 16,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 0, 10, 0)
                };

                Grid.SetColumn(quantityText, 1);
                rowGrid.Children.Add(quantityText);

                // Hover + Click Logic
                rowGrid.MouseEnter += (s, e) =>
                {
                    ItemDescriptionBlock.Text = item.Description;
                };

                rowGrid.MouseLeftButtonDown += (s, e) =>
                {
                    ShowPopup(item);
                };

                ItemListPanel.Children.Add(rowGrid);
            }
        }

        private void ShowPopup(InventoryItem item)
        {
            GameSystems.Inventory.SelectItem(item);
            selectedTargets.Clear();

            var fullParty = GameSystems.Party.AllCharacters;

            // Auto-preselect for "All" types
            switch (item.TargetType)
            {
                case TargetType.AllAllies:
                    selectedTargets.AddRange(fullParty.Where(c => c.IsAlive));
                    break;
                case TargetType.AllFaintedAllies:
                    selectedTargets.AddRange(fullParty.Where(c => !c.IsAlive));
                    break;
            }

            PopupQuestion.Text = $"Use {item.Name}";
            PopupIcon.Source = new BitmapImage(new Uri(AssetPathResolver.GetIconsPath(item.IconFile)));

            RefreshPortraitVisuals();

            PopupOverlay.Visibility = Visibility.Visible;
        }

        private bool IsCharacterValidTarget(InventoryItem item, Character character)
        {
            switch (item.TargetType)
            {
                case TargetType.Self:
                case TargetType.Ally:
                    return character.IsAlive;

                case TargetType.AllAllies:
                    return character.IsAlive;

                case TargetType.SingleFaintedAlly:
                    return !character.IsAlive;

                case TargetType.AllFaintedAllies:
                    return !character.IsAlive;

                // Ignored in inventory popup context
                case TargetType.Enemy:
                case TargetType.AllEnemies:
                default:
                    return false;
            }
        }

        private CharacterPortrait CreateCharacterPortraitWithExtras(Character character, bool isEnabled, bool isSelected)
        {
            return new CharacterPortrait
            {
                Character = character,
                IsEnabledTarget = isEnabled,
                IsSelected = isSelected,
                Margin = new Thickness(4),
                Cursor = isEnabled ? Cursors.Hand : Cursors.Arrow
            };
        }

        void RefreshPortraitVisuals()
        {
            PartyTargetPanel.Children.Clear();

            foreach (var character in partyManager.AllCharacters)
            {
                bool isValid = IsCharacterValidTarget(GameSystems.Inventory.SelectedItem, character);
                bool isSelected = selectedTargets.Contains(character);

                var portrait = CreateCharacterPortraitWithExtras(character, isValid, isSelected);
                portrait.IsEnabled = isValid;

                // Disable click if type is AllAllies or AllFaintedAllies or Self
                bool isSelectionLocked =
                    GameSystems.Inventory.SelectedItem.TargetType == TargetType.AllAllies ||
                    GameSystems.Inventory.SelectedItem.TargetType == TargetType.AllFaintedAllies;

                if (isValid && !isSelectionLocked)
                {
                    portrait.MouseLeftButtonUp += (s, e) =>
                    {
                        if (!selectedTargets.Contains(character))
                        {
                            if (GameSystems.Inventory.SelectedItem.TargetType is TargetType.Ally 
                            || GameSystems.Inventory.SelectedItem.TargetType is TargetType.SingleFaintedAlly
                            || GameSystems.Inventory.SelectedItem.TargetType is TargetType.Self)
                                selectedTargets.Clear(); // Only one target allowed

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

        private void PopupYes_Click(object sender, RoutedEventArgs e)
        {
            foreach (var target in selectedTargets)
            {
                GameSystems.Inventory.UseSelectedItem();
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