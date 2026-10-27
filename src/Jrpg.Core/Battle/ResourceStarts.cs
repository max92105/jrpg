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
    // A resource keeps at least one point of room, so Will taken away by gear never leaves nothing to fill.
    private const int LeastMax = 1;

    /// <returns>One start per class resource, in the class's order.</returns>
    public static IReadOnlyList<ResourceStart> For(PartyMember member) => [.. member.Class.Resources.Select(resource => StrainedStart(resource, member))];

    private static ResourceStart StrainedStart(ResourceDefinition resource, PartyMember member)
    {
        var start = WillRaised(StartOf(resource, member), member);
        return start with { Amount = StrainedAmount(start, member) };
    }

    // A strain's source is lasting (Data checks it), so the member holds its amount.
    private static int StrainedAmount(ResourceStart start, PartyMember member)
    {
        var lowering = member.Class.Resources.Sum(source =>
            source.Strain is { } strain && strain.LowersStartOf == start.Definition.Id
                ? (source.Max - member.LastingAmounts[source.Id]) * strain.PerMissingPoint
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

        var points = WillFormula.BonusPoints(member.Sheet.Primaries.Will, scaling.WillPerPoint);
        var max = scaling.Effect == WillEffect.RaiseMax ? Math.Max(LeastMax, start.Max + points) : start.Max;
        var amount = scaling.Effect switch
        {
            WillEffect.RaiseStart => start.Amount + points,
            WillEffect.RaiseMax => start.Amount == start.Max ? max : start.Amount,
            _ => throw new ArgumentOutOfRangeException(nameof(start), scaling.Effect, "Unknown Will effect."),
        };
        return start with { Max = max, Amount = Math.Clamp(amount, 0, max) };
    }
}
