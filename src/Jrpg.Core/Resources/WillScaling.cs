namespace Jrpg.Core.Resources;

/// <summary>
/// How a resource follows its holder's Will (decision 291): one point per <c>WillPerPoint</c>, applied as <c>Effect</c>
/// says.
/// </summary>
public sealed record WillScaling(int WillPerPoint, WillEffect Effect);
