using Jrpg.Core.Party;
using Jrpg.Core.Resources;

namespace Jrpg.Core.Battle;

/// <summary>
/// A member's resources as they start a battle: the definition's numbers, the amount a lasting resource carried from
/// the last battle (ADR 0024), or the speed's charges; then lowered by any strain on it. Public so a screen
/// outside battle (the Status page) shows the same maxes.
/// </summary>
public static class ResourceStarts
{
    /// <returns>One start per class resource, in the class's order.</returns>
    public static IReadOnlyList<ResourceStart> For(PartyMember member) => [.. member.Class.Resources.Select(resource => StrainedStart(resource, member))];

    private static ResourceStart StrainedStart(ResourceDefinition resource, PartyMember member)
    {
        var start = StartOf(resource, member);
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

        var charges = ReadinessFormula.Charges(member.Speed, scaling.SpeedPerCharge);
        return new ResourceStart(resource, charges, charges);
    }
}
