using JRPG.Core.Battles.Models;
using JRPG.Core.Systems.Equipments.Models;
using System.Windows.Controls;

namespace JRPG.UI.GameMenu.Controls
{
    /// <summary>A character's parameters, with a preview of what a piece of equipment would change.</summary>
    public partial class StatsPanel : UserControl
    {
        public StatsPanel()
        {
            InitializeComponent();
        }

        public void Load(Character character)
        {
            if (character == null)
                return;

            MaxHpLine.Show(character.MaxHP);
            MaxMpLine.Show(character.MaxMP);
            AttackLine.Show(character.GetTotalAttack());
            DefenseLine.Show(character.GetTotalDefense());
        }

        /// <summary>
        /// Shows what Attack / Defense would become with this equipment.
        /// Null equipment previews an empty slot.
        /// </summary>
        public void PreviewStatChange(Character character, Equipment equipment, bool isWeapon)
        {
            if (character == null)
                return;

            ClearPreview();

            if (isWeapon)
                AttackLine.ShowPreview(character.BaseAttack + ((equipment as Weapon)?.AttackBonus ?? 0));
            else
                DefenseLine.ShowPreview(character.BaseDefense + ((equipment as Armor)?.DefenseBonus ?? 0));
        }

        public void ClearPreview()
        {
            MaxHpLine.ClearPreview();
            MaxMpLine.ClearPreview();
            AttackLine.ClearPreview();
            DefenseLine.ClearPreview();
        }
    }
}
