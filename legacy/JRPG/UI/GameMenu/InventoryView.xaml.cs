using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JRPG.Core.Battles.Enums;
using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Systems.Inventories.Models;
using JRPG.Core.Utils;
using JRPG.UI.GameMenu.Controls;
using JRPG.Utils;

namespace JRPG.UI.GameMenu
{
    /// <summary>
    /// Inventory page: the item list, filtered by the category tabs, and the "Use item" dialog.
    /// Item descriptions show in the menu footer.
    /// </summary>
    public partial class InventoryView : MenuPage, IFilterable<Type>
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
            ShowHint(null);

            foreach (var entry in GameSystems.Inventory.Items)
            {
                if (selectedFilterType != null && !selectedFilterType.IsInstanceOfType(entry.Item))
                    continue;

                bool canUse = entry.Item.CanBeUsedFromInventory;

                var row = new ListRow
                {
                    Icon = ImageLoader.LoadIcon(entry.Item.IconFile),
                    Text = entry.Item.Name,
                    Value = entry.Quantity.ToString(),
                    Badge = entry.EquippedTo.Count > 0 ? "E" : null,
                    // Grey out consumables that only work in battle; equipment stays normal
                    IsDimmed = entry.Item is Consumable && !canUse,
                    IsClickable = canUse
                };

                row.MouseEnter += (s, e) => ShowHint(entry.Item.Description);
                row.MouseLeave += (s, e) => ShowHint(null);

                if (canUse)
                    row.MouseLeftButtonDown += (s, e) => ShowPopup(entry);

                ItemListPanel.Children.Add(row);
            }

            EmptyText.Visibility = ItemListPanel.Children.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
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
            PopupIcon.Source = ImageLoader.LoadIcon(item.Item.IconFile);

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
            NotifyPartyChanged();
        }

        private void PopupNo_Click(object sender, RoutedEventArgs e)
        {
            GameSystems.Inventory.ClearSelection();
            PopupOverlay.Visibility = Visibility.Collapsed;
        }
    }
}
