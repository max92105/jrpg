using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Systems.Equipments.Models;
using JRPG.UI.GameMenu.Controls;
using JRPG.Utils;
using System.Linq;
using System.Windows.Input;

namespace JRPG.UI.GameMenu
{
    /// <summary>Equipment page: a character's slots and parameters. The category tabs pick the character.</summary>
    public partial class EquipmentView : MenuPage, IFilterable<Character>
    {
        private const string EmptySlotText = "—";

        private Character character;

        public EquipmentView()
        {
            InitializeComponent();

            WeaponSlot.MouseEnter += (s, e) => ShowHint(character?.EquippedWeapon?.Description);
            ArmorSlot.MouseEnter += (s, e) => ShowHint(character?.EquippedArmor?.Description);
            WeaponSlot.MouseLeave += (s, e) => ShowHint(null);
            ArmorSlot.MouseLeave += (s, e) => ShowHint(null);

            ApplyFilter(GameSystems.Party.AllCharacters.FirstOrDefault());
        }

        public void ApplyFilter(Character newCharacter)
        {
            if (newCharacter == null)
                return;

            character = newCharacter;
            ClosePicker();
            Refresh();
        }

        private void Refresh()
        {
            CharacterNameText.Text = character.Name;
            ShowSlot(WeaponSlot, character.EquippedWeapon);
            ShowSlot(ArmorSlot, character.EquippedArmor);
            Stats.Load(character);
        }

        private static void ShowSlot(ListRow slot, Equipment equipped)
        {
            slot.Detail = equipped?.Name ?? EmptySlotText;
            slot.DetailIcon = equipped == null ? null : ImageLoader.LoadIcon(equipped.IconFile);
        }

        // ============ Slots ============

        private void WeaponSlot_Click(object sender, MouseButtonEventArgs e) => OpenPicker(WeaponSlot, isWeapon: true);

        private void ArmorSlot_Click(object sender, MouseButtonEventArgs e) => OpenPicker(ArmorSlot, isWeapon: false);

        private void RemoveAll_Click(object sender, MouseButtonEventArgs e)
        {
            ClosePicker();

            GameSystems.Equipment.Unequip(character, character.EquippedWeapon);
            GameSystems.Equipment.Unequip(character, character.EquippedArmor);

            Refresh();
        }

        // ============ Picker (right panel) ============

        private void OpenPicker(ListRow slot, bool isWeapon)
        {
            WeaponSlot.IsSelected = slot == WeaponSlot;
            ArmorSlot.IsSelected = slot == ArmorSlot;

            var picker = new EquipmentSelectionView(character, isWeapon);
            picker.Previewing += equipment => Stats.PreviewStatChange(character, equipment, isWeapon);
            picker.PreviewEnded += Stats.ClearPreview;
            picker.HintRequested += ShowHint;
            picker.Done += () =>
            {
                ClosePicker();
                Refresh();
            };

            ShowSecondary(picker);
        }

        private void ClosePicker()
        {
            WeaponSlot.IsSelected = false;
            ArmorSlot.IsSelected = false;
            Stats.ClearPreview();
            HideSecondary();
        }
    }
}
