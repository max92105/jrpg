using Jrpg.Core.Content;
using Jrpg.Core.Maps;

namespace Jrpg.Game.Flow;

/// <summary>
/// Random battles stay off until the game can show a battle (developer decision 79). The game plays a copy of the
/// content without random battle tables, so Core never rolls one, while the files and the editor keep them.
/// </summary>
// TODO(plan 2026-10-18-game-title-menu-movement, decision 79): delete once the battle plan shows random battles.
internal static class EncounterFreeContent
{
    public static ContentCatalog Of(ContentCatalog catalog) => catalog with
    {
        Maps = catalog.Maps.ToDictionary(
            entry => entry.Key,
            entry => new MapDefinition(entry.Value.Layout, entry.Value.Settings with { RandomEncounters = null })),
    };
}
