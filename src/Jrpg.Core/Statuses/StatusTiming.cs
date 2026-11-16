namespace Jrpg.Core.Statuses;

/// <summary>
/// When a status's tick acts in its holder's turn (decision 695); its countdown stays at the turn's end (ADR 0021).
/// </summary>
public enum StatusTiming
{
    /// <summary>As the holder's turn starts, before a stun skips it; never on a readied extra turn.</summary>
    StartOfTurn,

    /// <summary>As the holder's turn ends, before the countdown.</summary>
    EndOfTurn,
}
