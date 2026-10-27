namespace Jrpg.Core.Resources;

/// <summary>What a holder's Will does to one of their resources (decision 319).</summary>
public enum WillEffect
{
    /// <summary>The points are added to the amount the resource starts a battle with, within its max.</summary>
    RaiseStart,

    /// <summary>
    /// The points are added to the max, and to the start too when the resource starts full; a lasting resource refills
    /// to the raised max at a rest.
    /// </summary>
    RaiseMax,

    /// <summary>Every gain to the resource, from a skill or a trigger, is lower by the points, never below 1.</summary>
    LowerGain,
}
