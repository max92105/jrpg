namespace Jrpg.Core.Resources;

/// <summary>
/// A resource whose max and starting amount come from its holder's speed: one charge per <c>SpeedPerCharge</c>.
/// </summary>
public sealed record SpeedScaling(int SpeedPerCharge);
