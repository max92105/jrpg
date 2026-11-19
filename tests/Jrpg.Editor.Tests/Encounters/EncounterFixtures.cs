using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Maps;
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

    /// <summary>No map at all, for the tests that link nothing.</summary>
    public static EncounterMapLinks NoMaps => new(() => [], _ => { });

    /// <summary>An empty map with these random battles, fought on the Cave background.</summary>
    public static MapDefinition MapRolling(string id, params string[] encounterIds)
    {
        var map = MapWithoutBattles(id, "Cave.png");
        var table = new RandomEncounterTable { OneInSteps = 12, GraceSteps = 3, EncounterIds = encounterIds };
        return new MapDefinition(map.Layout, map.Settings with { RandomEncounters = table });
    }

    /// <summary>An empty map without random battles, named as its id in upper case.</summary>
    public static MapDefinition MapWithoutBattles(string id, string? background) =>
        new(
            new MapLayout { Id = id, Width = 1, Height = 1, Layers = [], Objects = [] },
            new MapSettings { Id = id, DisplayName = id.ToUpperInvariant(), BattleBackgroundFile = background });

    private static ContentCatalog CatalogOf(ContentCatalog world)
    {
        var goblin = world.Enemies["gnoll"] with { Id = "goblin", Name = "Goblin" };
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
