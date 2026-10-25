using Jrpg.Core.Party;
using Jrpg.Core.Resources;

namespace Jrpg.Core.Battle;

/// <summary>
/// A member's resources as they start a battle: the definition's numbers, the amount a lasting resource carried from
/// the last battle (ADR 0024), or the speed's charges.
/// </summary>
internal static class ResourceStarts
{
    public static IReadOnlyList<ResourceStart> For(PartyMember member) => [.. member.Class.Resources.Select(resource => StartOf(resource, member))];

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
