namespace Jrpg.Core.Statuses;

/// <summary>The closed list of statuses; each kind's rule is code with a test (ADR 0021).</summary>
public enum StatusKind
{
    /// <summary>The holder loses its next turn (ST2).</summary>
    Stun,

    /// <summary>Every enemy attack goes to a taunting party member (question 209).</summary>
    Taunt,

    /// <summary>
    /// Every resistance of the holder rises by the status's power, in points, while it lasts (the paladin's aura).
    /// </summary>
    Guarded,

    /// <summary>Every hit on the holder lands harder by the status's power, in percent (question 215).</summary>
    Exposed,

    /// <summary>The holder loses the status's power in HP at the end of each of its turns, past resistances.</summary>
    Poison,

    /// <summary>The holder regains the status's power in HP at the end of each of its turns.</summary>
    Regen,
}
