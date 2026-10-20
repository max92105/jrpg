using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Data.Art;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Art;

public sealed class ArtReferenceCheckTests
{
    private static readonly string _artRoot = ArtFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "content"));
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void FindProblems_RepositoryContent_FindsNone()
    {
        Assert.Empty(ArtReferenceCheck.FindProblems(_catalog, _artRoot));
    }

    [Fact]
    public void FindProblems_CellOutsideSheet_NamesTheTile()
    {
        // The forest sheet is cut in 16-pixel cells; column 1000 is far past its right edge.
        var tile = _catalog.Tiles.Values.First(tile => tile.SheetId == "fg_forest_fall");
        var catalog = _catalog with { Tiles = new Dictionary<int, TileDefinition> { [tile.Id] = tile with { Column = 1000 } } };

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(catalog, _artRoot));

        Assert.StartsWith($"tilesheets/FG_Forest_Fall.png: tile {tile.Id}'s cell (1000, {tile.Row}) lies outside the ", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void FindProblems_MissingFieldSprite_NamesTheFile()
    {
        var catalog = _catalog with { NewGame = _catalog.NewGame with { FieldSpriteFile = "ghost.png" } };

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(catalog, _artRoot));

        Assert.Equal($"field-sprites/ghost.png: not found in {_artRoot}.", problem);
    }

    [Fact]
    public void FindProblems_MissingDialoguePortrait_NamesTheEvent()
    {
        var start = _catalog.Maps["start"];
        var chest = start.Layout.Objects.First(placed => placed.ObjectId == "chest");
        var speech = new Speech(["Who goes there?"], Speaker: "Ghost", PortraitFile: "ghost.png");
        var talkingChest = chest with { Events = [chest.Events[0] with { Steps = [new DialogueStep(speech)] }] };
        var layout = start.Layout with { Objects = [.. start.Layout.Objects.Select(placed => placed == chest ? talkingChest : placed)] };
        var catalog = _catalog with
        {
            Maps = new Dictionary<string, MapDefinition>(_catalog.Maps) { ["start"] = new MapDefinition(layout, start.Settings) },
        };

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(catalog, _artRoot));

        Assert.Equal($"portraits/ghost.png: not found in {_artRoot}. Named by event '{chest.Events[0].Id}' on map 'start'.", problem);
    }

    [Fact]
    public void FindProblems_MissingPartyPortrait_NamesTheMember()
    {
        var jazog = _catalog.PartyMembers["jazog"] with { PortraitFile = "Gone.png" };
        var catalog = _catalog with { PartyMembers = new Dictionary<string, PartyMemberDefinition>(_catalog.PartyMembers) { ["jazog"] = jazog } };

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(catalog, _artRoot));

        Assert.Equal($"portraits/Gone.png: not found in {_artRoot}. Named by party member 'jazog'.", problem);
    }

    [Fact]
    public void FindProblems_MissingItemIcon_NamesTheItem()
    {
        var potion = _catalog.Items["potion"] with { IconFile = "Gone.png" };
        var catalog = _catalog with { Items = new Dictionary<string, ItemDefinition>(_catalog.Items) { ["potion"] = potion } };

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(catalog, _artRoot));

        Assert.Equal($"icons/Gone.png: not found in {_artRoot}. Named by item 'potion'.", problem);
    }

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

        Assert.Equal("backgrounds/forest.png: the file is named Forest.png; names must match exactly. Named by encounter 'goblin_trio'.", problem);
    }

    [Fact]
    public void FindProblems_MissingTilesheet_NamesTheFileAndSkipsItsTiles()
    {
        var sheet = _catalog.Tilesheets["city"];
        var catalog = _catalog with { Tilesheets = new Dictionary<string, TilesheetDefinition>(_catalog.Tilesheets) { ["city"] = sheet with { File = "Gone.png" } } };

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(catalog, _artRoot));

        Assert.Equal($"tilesheets/Gone.png: not found in {_artRoot}.", problem);
    }

    private static ContentCatalog WithGoblinTrioBackground(string file)
    {
        var trio = _catalog.Encounters["goblin_trio"] with { BackgroundFile = file };
        return _catalog with { Encounters = new Dictionary<string, EncounterDefinition>(_catalog.Encounters) { ["goblin_trio"] = trio } };
    }
}
