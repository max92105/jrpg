namespace Jrpg.Core.Stats;

/// <summary>Which group a damage type is in: Constitution raises physical resistances, Will magical ones.</summary>
public static class DamageTypes
{
    public static bool IsPhysical(DamageType type) => type is DamageType.Blunt or DamageType.Piercing or DamageType.Slashing;
}
