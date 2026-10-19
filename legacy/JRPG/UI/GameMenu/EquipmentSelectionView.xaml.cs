using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Systems.Equipments.Models;
using JRPG.UI.GameMenu.Controls;
using JRPG.Utils;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JRPG.UI.GameMenu
{
    /// <summary>
    /// Lists the equipment a character can put in one slot. Created by EquipmentView,
    /// which listens to the events below to preview stats and close the picker.
    /// </summary>
    public partial class EquipmentSelectionView : UserControl
    {
        private const double LockedListOpacity = 0.4;

        /// <summary>Hovering a choice. Null = the "Remove" choice (empty slot).</summary>
        public event Action<Equipment> Previewing;
        public event Action PreviewEnded;
        public event Action<string> HintRequested;
        /// <summary>The slot was changed; the picker can close.</summary>
        public event Action Done;

        private readonly Character character;
        private readonly bool isWeapon;

        // Set when the player picked an item everyone already wears, and must choose who gives it up
        private Equipment itemToTake;

        public EquipmentSelectionView(Character character, bool isWeapon)
        {
            InitializeComponent();
            this.character = character;
            this.isWeapon = isWeapon;

            HeaderText.Text = isWeapon ? "Weapon" : "Armor";
            BuildList();
        }

        private Equipment EquippedInSlot => isWeapon ? (Equipment)character.EquippedWeapon : character.EquippedArmor;

        private bool IsChoosingWhoToTakeFrom => itemToTake != null;

        private void BuildList()
        {
            EquipmentListPanel.Children.Clear();
            EquipmentListPanel.Children.Add(CreateRemoveRow());

            var entries = GameSystems.Inventory.Items.Where(e =>
                e.Item is Equipment equipment &&
                equipment.AllowedClasses.Contains(character.Class) &&
                (isWeapon ? equipment is Weapon : equipment is Armor));

            foreach (var entry in entries)
            {
                var equipment = (Equipment)entry.Item;
                bool wornByCharacter = entry.EquippedTo.Contains(character);
                bool isAvailable = entry.AvailableStock > 0;

                var row = new ListRow
                {
                    Icon = ImageLoader.LoadIcon(equipment.IconFile),
                    Text = equipment.Name,
                    Value = entry.Quantity.ToString(),
                    Badge = wornByCharacter ? "E" : null,
                    IsDimmed = wornByCharacter || !isAvailable,
                    IsClickable = !wornByCharacter
                };

                row.MouseEnter += (s, e) => StartPreview(equipment, equipment.Description);
                row.MouseLeave += (s, e) => EndPreview();

                if (!wornByCharacter)
                {
                    row.MouseLeftButtonUp += (s, e) =>
                    {
                        if (IsChoosingWhoToTakeFrom)
                            return;

                        if (isAvailable)
                            EquipAndFinish(equipment);
                        else
                            AskWhoToTakeFrom(equipment);
                    };
                }

                EquipmentListPanel.Children.Add(row);
            }
        }

        private ListRow CreateRemoveRow()
        {
            var row = new ListRow { Text = "Remove", IsClickable = true };

            row.MouseEnter += (s, e) => StartPreview(null, "Leave this slot empty.");
            row.MouseLeave += (s, e) => EndPreview();
            row.MouseLeftButtonUp += (s, e) =>
            {
                if (IsChoosingWhoToTakeFrom)
                    return;

                GameSystems.Equipment.Unequip(character, EquippedInSlot);
                Done?.Invoke();
            };

            return row;
        }

        // ============ Hover preview ============

        private void StartPreview(Equipment equipment, string hint)
        {
            if (IsChoosingWhoToTakeFrom)
                return;

            Previewing?.Invoke(equipment);
            HintRequested?.Invoke(hint);
            ShowEquippedBy(equipment, canTake: false);
        }

        private void EndPreview()
        {
            if (IsChoosingWhoToTakeFrom)
                return;

            PreviewEnded?.Invoke();
            HintRequested?.Invoke(null);
            ShowEquippedBy(null, canTake: false);
        }

        // ============ Equipping ============

        private void EquipAndFinish(Equipment equipment)
        {
            GameSystems.Equipment.Equip(character, equipment);
            Done?.Invoke();
        }

        /// <summary>Every copy is worn: the player picks a party member to take it from.</summary>
        private void AskWhoToTakeFrom(Equipment equipment)
        {
            itemToTake = equipment;
            EquipmentListPanel.Opacity = LockedListOpacity;
            HintRequested?.Invoke("Every copy is worn. Choose who gives it up.");
            ShowEquippedBy(equipment, canTake: true);
        }

        private void ShowEquippedBy(Equipment equipment, bool canTake)
        {
            EquippedByPanel.Children.Clear();

            var wearers = GameSystems.Inventory.Items.FirstOrDefault(e => e.Item == equipment)?.EquippedTo;
            EquippedBySection.Visibility = wearers != null && wearers.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            if (wearers == null)
                return;

            foreach (var wearer in wearers.ToList())
            {
                var portrait = new CharacterPortrait
                {
                    Character = wearer,
                    ShowStats = false,
                    IsEnabledTarget = canTake,
                    Margin = new Thickness(0, 4, 8, 0),
                    Cursor = canTake ? Cursors.Hand : Cursors.Arrow,
                    ToolTip = wearer.Name
                };

                if (canTake)
                {
                    portrait.MouseLeftButtonUp += (s, e) =>
                    {
                        GameSystems.Equipment.Unequip(wearer, itemToTake);
                        EquipAndFinish(itemToTake);
                    };
                }

                EquippedByPanel.Children.Add(portrait);
            }
        }
    }
}
