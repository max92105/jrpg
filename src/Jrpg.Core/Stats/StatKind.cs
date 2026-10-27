namespace Jrpg.Core.Stats;

/// <summary>
/// The twenty stats a sheet shows and gear can add (ADR 0026): the five primaries, the derived numbers, and one
/// resistance per damage type (<see cref="StatKinds.ResistanceTo"/>).
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
}
