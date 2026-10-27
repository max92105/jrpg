namespace Jrpg.Core.Resources;

/// <summary>What a holder's Will does to one of their resources (decision 319).</summary>
public enum WillEffect
{
    /// <summary>The points are added to the amount the resource starts a battle with, within its max.</summary>
    RaiseStart,

    /// <summary>The points are added to the max, and to the start too when the resource starts full.</summary>
    RaiseMax,
}
