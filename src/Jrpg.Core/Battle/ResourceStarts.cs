using Jrpg.Core.Party;
using Jrpg.Core.Resources;

namespace Jrpg.Core.Battle;

/// <summary>
/// A member's resources as they start a battle: the definition's numbers, the amount a lasting resource carried from
/// the last battle (ADR 0024), or the speed's charges; raised by the member's Will where the resource says so (decision
/// 319); then lowered by any strain on it. Public so a screen outside battle (the Status page) shows the same maxes.
/// </summary>
public static class ResourceStarts
{
    /// <returns>One start per class resource, in the class's order.</returns>
    public static IReadOnlyList<ResourceStart> For(PartyMember member) => [.. member.Class.Resources.Select(resource => StrainedStart(resource, member))];

    private static ResourceStart StrainedStart(ResourceDefinition resource, PartyMember member)
    {
        var start = WillRaised(StartOf(resource, member), member);
        return start with { Amount = StrainedAmount(start, member) };
    }

    // A strain's source is lasting (Data checks it), so the member holds its amount, and its missing points are counted
    // from the max its holder's Will gives it.
    private static int StrainedAmount(ResourceStart start, PartyMember member)
    {
        var lowering = member.Class.Resources.Sum(source =>
            source.Strain is { } strain && strain.LowersStartOf == start.Definition.Id
                ? (MaxUnderWill(source, member) - member.LastingAmounts[source.Id]) * strain.PerMissingPoint
                : 0);
        return Math.Max(0, start.Amount - lowering);
    }

    private static ResourceStart StartOf(ResourceDefinition resource, PartyMember member)
    {
        if (resource.MaxFromSpeed is not { } scaling)
        {
            return resource.IsLasting
                ? new ResourceStart(resource, resource.Max, member.LastingAmounts[resource.Id])
                : ResourceStart.Fixed(resource);
        }

        var charges = ReadinessFormula.Charges(member.Sheet.Speed, scaling.SpeedPerCharge);
        return new ResourceStart(resource, charges, charges);
    }

    // A raised max also raises the start of a resource that starts full, as the speed charges do, so the holder feels
    // the points from the first turn; a raised start stays within the max.
    private static ResourceStart WillRaised(ResourceStart start, PartyMember member)
    {
        if (start.Definition.WillScaling is not { } scaling)
        {
            return start;
        }

        var will = member.Sheet.Primaries.Will;
        var points = WillFormula.BonusPoints(will, scaling.WillPerPoint);
        var max = WillFormula.MaxUnder(start.Max, scaling, will);
        return scaling.Effect switch
        {
            WillEffect.RaiseStart => start with { Amount = Math.Clamp(start.Amount + points, 0, max) },
            WillEffect.RaiseMax => start with { Max = max, Amount = StartsFull(start) ? max : Math.Min(start.Amount, max) },
            WillEffect.LowerGain => start with { GainLowering = Math.Max(0, points) },
            _ => throw new ArgumentOutOfRangeException(nameof(start), scaling.Effect, "Unknown Will effect."),
        };
    }

    private static int MaxUnderWill(ResourceDefinition resource, PartyMember member) =>
        WillFormula.MaxUnder(resource.Max, resource.WillScaling, member.Sheet.Primaries.Will);

    // A lasting resource's amount is what the member holds, already within the raised max, never a fresh fill.
    private static bool StartsFull(ResourceStart start) => !start.Definition.IsLasting && start.Amount == start.Max;
}
