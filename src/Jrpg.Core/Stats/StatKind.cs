namespace Jrpg.Core.Stats;

/// <summary>
/// The stats a sheet shows and gear can add (ADR 0026): the five primaries, the derived numbers, one resistance per
/// damage type (<see cref="StatKinds.ResistanceTo"/>), then the stats only affixes give
/// (<see cref="StatKinds.AffixOnly"/>, ADR 0030).
/// </summary>
public enum StatKind
{
    Power,
    Agility,
    Intelligence,
    Constitution,
    Will,
    MaxHp,
    Speed,
    Accuracy,
    CritChance,
    CritDamage,
    Dodge,
    CrowdControlResistance,
    BluntResistance,
    PiercingResistance,
    SlashingResistance,
    FireResistance,
    IceResistance,
    LightningResistance,
    LightResistance,
    DarkResistance,
    LifeSteal,
    ArmorPenetration,
}
