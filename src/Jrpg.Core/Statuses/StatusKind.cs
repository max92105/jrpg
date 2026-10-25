namespace Jrpg.Core.Statuses;

/// <summary>The closed list of statuses; each kind's rule is code with a test (ADR 0021).</summary>
public enum StatusKind
{
    /// <summary>The holder loses its next turn (ST2).</summary>
    Stun,
}
