namespace Jrpg.Core.Statuses;

/// <summary>
/// What a part of a status does while held: the closed list a status record is built from (ADR 0055, decision 796);
/// each part's amount means what its kind says.
/// </summary>
public enum StatusPartKind
{
    /// <summary>The holder loses the amount in HP at each tick.</summary>
    DamageEachTurn,

    /// <summary>The holder regains the amount in HP at each tick.</summary>
    HealEachTurn,

    /// <summary>The holder loses its turns while it holds the status; no amount.</summary>
    SkipTurn,

    /// <summary>
    /// Every enemy attack goes to the holder, unless crowd control resistance shrugs it off; no amount.
    /// </summary>
    DrawAttacks,

    /// <summary>Every resistance of the holder changes by the amount, in points.</summary>
    ResistanceChange,

    /// <summary>
    /// Every hit on the holder lands the amount harder, in percent; a negative one softer, above -100.
    /// </summary>
    DamageTakenChange,

    /// <summary>
    /// Every damage the holder deals changes by the amount, in percent added to its damage modifiers' sum (decision
    /// 786); a negative one lowers it, above -100.
    /// </summary>
    DamageDealtChange,

    /// <summary>
    /// Its holder's <see cref="StatusPart.Stat"/> changes by the amount, in percentage points (decision 807).
    /// </summary>
    StatChange,
}
