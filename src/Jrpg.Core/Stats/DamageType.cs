namespace Jrpg.Core.Stats;

/// <summary>
/// What a hit is (ADR 0027): every hit has exactly one, from content. Blunt, Piercing and Slashing are physical, the
/// rest magical (<see cref="DamageTypes.IsPhysical"/>).
/// </summary>
public enum DamageType
{
    Blunt,
    Piercing,
    Slashing,
    Fire,
    Ice,
    Lightning,
    Light,
    Dark,
}
