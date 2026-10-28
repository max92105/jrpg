using Jrpg.Core.Battle;
using Jrpg.Core.Loot;

namespace Jrpg.Core.Session;

/// <summary>
/// What a won battle gave: the experience, and the drops beside it, so the battle itself knows nothing of loot (ADR
/// 0030).
/// </summary>
/// <param name="Rewards">The experience, as the battle granted it.</param>
/// <param name="Drops">What was added to the inventory: copies past a stack's limit are lost and not listed.</param>
public sealed record BattleSpoils(VictoryRewards Rewards, Drops Drops);
