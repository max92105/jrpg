namespace Jrpg.Core.Battle;

/// <summary>Whom a rule's trigger names as "that one" (decision 839), which its target may then aim at.</summary>
public enum TacticSubject
{
    /// <summary>The enemy itself.</summary>
    Itself,

    /// <summary>The living allies the trigger found.</summary>
    Allies,

    /// <summary>The fallen allies.</summary>
    FallenAllies,

    /// <summary>The foes the trigger found.</summary>
    Foes,
}
