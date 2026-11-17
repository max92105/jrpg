using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Encounters;

namespace Jrpg.Editor.Tests.Encounters;

/// <summary>
/// The Encounters page tests' own enemies, folders and encounters, so no repository record or file is pinned.
/// </summary>
internal static class EncounterFixtures
{
    // Loaded at the first read, so a load failure fails only the tests reading the catalog.
    private static readonly Lazy<ContentCatalog> _catalog =
        new(() => CatalogOf(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"))));

    public static EnemyChoice Goblin { get; } = new("goblin", "Goblin");

    public static EnemyChoice Orc { get; } = new("orc", "Orc");

    /// <summary>Two backgrounds and one track in the folders.</summary>
    public static EncounterChoices Choices { get; } = new([Goblin, Orc], ["Cave.png", "Forest.png"], ["boss/loop.wav"]);

    /// <summary>Two goblins: two slots whose rows are alike but for their reference.</summary>
    public static EncounterDefinition GoblinPair { get; } = new() { Id = "goblin_pair", EnemyIds = ["goblin", "goblin"] };

    /// <summary>
    /// The test world with the goblin and an orc for enemies, and for encounters b_pair (two goblins) then a_trio
    /// (three orcs), out of id order.
    /// </summary>
    public static ContentCatalog Catalog => _catalog.Value;

    private static ContentCatalog CatalogOf(ContentCatalog world)
    {
        var goblin = world.Enemies["goblin"];
        return world with
        {
            Enemies = new Dictionary<string, EnemyDefinition>(StringComparer.Ordinal)
            {
                ["goblin"] = goblin,
                ["orc"] = goblin with { Id = "orc", Name = "Orc" },
            },
            Encounters = new Dictionary<string, EncounterDefinition>(StringComparer.Ordinal)
            {
                ["b_pair"] = GoblinPair with { Id = "b_pair" },
                ["a_trio"] = new() { Id = "a_trio", EnemyIds = ["orc", "orc", "orc"] },
            },
        };
    }
}
