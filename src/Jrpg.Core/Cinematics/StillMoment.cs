namespace Jrpg.Core.Cinematics;

/// <summary>Which still a cinematic shows at a time, and how far into it (decision 629).</summary>
/// <param name="StillIndex">The still's place in the cinematic, from 0.</param>
/// <param name="IntoStill">The time since that still began.</param>
public readonly record struct StillMoment(int StillIndex, TimeSpan IntoStill);
