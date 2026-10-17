using Jrpg.Core.Items;

namespace Jrpg.Core.Battle;

/// <summary>Use <c>Item</c> on <c>Targets</c>: one target for a single-target item, every valid target for an "all" item.</summary>
public sealed record ItemCommand(ItemDefinition Item, IReadOnlyList<Combatant> Targets) : BattleCommand;
