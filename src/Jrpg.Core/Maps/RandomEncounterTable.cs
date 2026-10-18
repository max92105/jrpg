namespace Jrpg.Core.Maps;

/// <summary>A map's random battles (developer decision 26): how often a step starts one, and which.</summary>
public sealed record RandomEncounterTable
{
    /// <summary>Each step past the grace steps starts a battle with a chance of 1 in this many.</summary>
    public required int OneInSteps { get; init; }

    /// <summary>Steps after entering the map or after any fight that can't start a battle, so battles never come back to back.</summary>
    public required int GraceSteps { get; init; }

    /// <summary>The encounters a battle is picked from, each as likely as the others.</summary>
    public required IReadOnlyList<string> EncounterIds { get; init; }
}
