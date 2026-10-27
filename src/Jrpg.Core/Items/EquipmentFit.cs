using System.Collections.Immutable;

namespace Jrpg.Core.Items;

/// <summary>
/// Which slots each kind of equipment fits (ADR 0028): a ring both ring slots, every other kind its own. Until the
/// member has the nine slots (G1a-2), only a weapon and a chest piece fit anywhere; the other kinds fit nothing, so
/// none can be equipped and content refuses them.
/// </summary>
public static class EquipmentFit
{
    /// <returns>
    /// The slots an item of <paramref name="kind"/> may be worn in; empty for a kind no slot holds yet.
    /// </returns>
    public static ImmutableArray<EquipmentSlot> SlotsFor(EquipmentKind kind) => kind switch
    {
        EquipmentKind.Weapon => [EquipmentSlot.Weapon],
        EquipmentKind.Chest => [EquipmentSlot.Chest],
        _ => [],
    };

    public static bool Fits(EquipmentKind kind, EquipmentSlot slot) => SlotsFor(kind).Contains(slot);
}
