using Jrpg.Core.Party;
using Jrpg.Core.Resources;

namespace Jrpg.Core.Battle;

/// <summary>A member's resources as they start a battle: the definition's numbers, or the speed's charges.</summary>
internal static class ResourceStarts
{
    public static IReadOnlyList<ResourceStart> For(PartyMember member) => [.. member.Class.Resources.Select(resource => StartOf(resource, member.Speed))];

    private static ResourceStart StartOf(ResourceDefinition resource, int speed)
    {
        if (resource.MaxFromSpeed is not { } scaling)
        {
            return ResourceStart.Fixed(resource);
        }

        var charges = ReadinessFormula.Charges(speed, scaling.SpeedPerCharge);
        return new ResourceStart(resource, charges, charges);
    }
}
