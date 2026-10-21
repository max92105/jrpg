using Jrpg.Core.Items;
using Jrpg.Game.Saving;

namespace Jrpg.Game.Battle;

/// <summary>What the battle screen reads from outside the battle.</summary>
/// <param name="Items">Every item by id, from the catalog, for the party's items.</param>
/// <param name="Saves">The save files the defeat screen loads from.</param>
internal sealed record BattleSceneSources(IReadOnlyDictionary<string, ItemDefinition> Items, SaveSlots Saves);
