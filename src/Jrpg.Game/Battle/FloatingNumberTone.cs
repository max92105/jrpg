namespace Jrpg.Game.Battle;

/// <summary>What a floating number reports, which sets its color.</summary>
internal enum FloatingNumberTone
{
    /// <summary>HP lost to a hit or a backfire.</summary>
    Damage,

    /// <summary>HP restored or revived with, Regen included.</summary>
    Heal,

    /// <summary>HP lost to Poison at the end of a turn.</summary>
    Poison,
}
