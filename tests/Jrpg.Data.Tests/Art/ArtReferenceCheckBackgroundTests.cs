using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Art;

// Battle backgrounds (decision 895) on the frozen test world (ADR 0037), whose art is otherwise complete: an
// encounter's own and a map's, by exact name.
public sealed class ArtReferenceCheckBackgroundTests
{
    private static readonly string _artRoot = ArtFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    [Fact]
    public void FindProblems_MissingEncounterBackground_NamesTheEncounter()
    {
        var catalog = WithGoblinTrioBackground("Gone.png");

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(catalog, _artRoot));

        Assert.Equal($"backgrounds/Gone.png: not found in {_artRoot}. Named by encounter 'goblin_trio'.", problem);
    }

    [Fact]
    public void FindProblems_BackgroundNameDiffersInCase_NamesTheFile()
    {
        var catalog = WithGoblinTrioBackground("forest.png");

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(catalog, _artRoot));

        Assert.Equal(
            "backgrounds/forest.png: the file is named Forest.png; names must match exactly. "
            + "Named by encounter 'goblin_trio'.",
            problem);
    }

    [Fact]
    public void FindProblems_EncounterWithoutOwnBackground_NoProblem()
    {
        var catalog = WithGoblinTrioBackground(null);

        var problems = ArtReferenceCheck.FindProblems(catalog, _artRoot);

        Assert.Empty(problems);
    }

    [Fact]
    public void FindProblems_MapBackgroundMissing_NamesTheMap()
    {
        var catalog = WithMapBackgrounds(("forest", "Gone.png"));

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(catalog, _artRoot));

        Assert.Equal($"backgrounds/Gone.png: not found in {_artRoot}. Named by map 'forest'.", problem);
    }

    [Fact]
    public void FindProblems_MapBackgroundWrongCase_IsAProblem()
    {
        var catalog = WithMapBackgrounds(("forest", "forest.png"));

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(catalog, _artRoot));

        Assert.Equal(
            "backgrounds/forest.png: the file is named Forest.png; names must match exactly. Named by map 'forest'.",
            problem);
    }

    // The maps are put in the catalog in the opposite order, so only the check's sort lists them by id.
    [Fact]
    public void FindProblems_TwoMapBackgroundsMissing_ListsThemInIdOrder()
    {
        var catalog = WithMapBackgrounds(("to_aslas_1", "Lost.png"), ("forest", "Gone.png"));

        var problems = ArtReferenceCheck.FindProblems(catalog, _artRoot);

        Assert.Equal(
            [
                $"backgrounds/Gone.png: not found in {_artRoot}. Named by map 'forest'.",
                $"backgrounds/Lost.png: not found in {_artRoot}. Named by map 'to_aslas_1'.",
            ],
            problems);
    }

    private static ContentCatalog WithGoblinTrioBackground(string? file)
    {
        var trio = _catalog.Encounters["goblin_trio"] with { BackgroundFile = file };
        var encounters = new Dictionary<string, EncounterDefinition>(_catalog.Encounters) { ["goblin_trio"] = trio };
        return _catalog with { Encounters = encounters };
    }

    // The changed maps first, in the order given, then the rest.
    private static ContentCatalog WithMapBackgrounds(params (string MapId, string File)[] changes)
    {
        var maps = new Dictionary<string, MapDefinition>();
        foreach (var (mapId, file) in changes)
        {
            var map = _catalog.Maps[mapId];
            maps.Add(mapId, new MapDefinition(map.Layout, map.Settings with { BattleBackgroundFile = file }));
        }

        foreach (var (mapId, map) in _catalog.Maps)
        {
            maps.TryAdd(mapId, map);
        }

        return _catalog with { Maps = maps };
    }
}
