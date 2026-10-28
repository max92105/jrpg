using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// What the Equipment page's Changes section compares (decision 337): with a choice open, the member now against the
/// highlighted item on (or the slot emptied for "Remove"); with none, what the highlighted slot's worn item gives, the
/// member without it against with it. The page rebuilds it only when what is compared changes.
/// </summary>
internal sealed class EquipmentComparison(UiMessages messages)
{
    /// <summary>What differs; null when there is nothing to compare (an empty slot, "Remove all").</summary>
    public StatComparison? Shown { get; private set; }

    /// <summary><see cref="Shown"/> as the page writes it.</summary>
    public StatComparisonText? Text { get; private set; }

    /// <summary>The member against the highlighted item in <paramref name="slot"/>, or the slot emptied.</summary>
    public void ShowChoice(PartyMember member, EquipmentSlot slot, EquipmentChoice choice)
    {
        var after = choice.Highlighted is { } piece ? member.SheetWith(piece, slot) : member.SheetWithout(slot);
        Show(StatComparison.Between(member.Sheet, after));
    }

    /// <summary>What the item worn in <paramref name="slot"/> gives; nothing for an empty slot or no slot.</summary>
    public void ShowWorn(PartyMember member, EquipmentSlot? slot)
    {
        if (slot is not { } worn || member.EquippedIn(worn) is null)
        {
            Shown = null;
            Text = null;
            return;
        }

        Show(StatComparison.Between(member.SheetWithout(worn), member.Sheet));
    }

    private void Show(StatComparison comparison)
    {
        Shown = comparison;
        Text = StatComparisonText.Of(comparison, messages);
    }
}
