namespace Jrpg.Core.Statuses;

/// <summary>The closed list of statuses; each kind's rule is code with a test (ADR 0021).</summary>
public enum StatusKind
{
    /// <summary>The holder loses its next turn (ST2).</summary>
    Stun,

    /// <summary>Every enemy attack goes to a taunting party member (question 209).</summary>
    Taunt,

    /// <summary>The holder's defense rises by the status's power while it lasts (the paladin's aura).</summary>
    Guarded,
}
