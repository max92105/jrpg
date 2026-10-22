using System.Buffers.Binary;
using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
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
    public void FindProblems_MissingObjectFieldSprite_NamesTheObject()
    {
        var villager = _catalog.Objects["villager"] with { FieldSpriteFile = "Gone.png" };
        var catalog = _catalog with { Objects = new Dictionary<string, ObjectDefinition>(_catalog.Objects) { ["villager"] = villager } };

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(catalog, _artRoot));

        Assert.Equal($"field-sprites/Gone.png: not found in {_artRoot}. Named by object 'villager'.", problem);
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
    public void FindProblems_SkillIconNameDiffersInCase_NamesTheSkill()
    {
        var fireBlast = _catalog.Skills["fire_blast"] with { IconFile = "attack.png" };
        var catalog = _catalog with { Skills = new Dictionary<string, SkillDefinition>(_catalog.Skills) { ["fire_blast"] = fireBlast } };

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(catalog, _artRoot));

        Assert.Equal("icons/attack.png: the file is named Attack.png; names must match exactly. Named by skill 'fire_blast'.", problem);
    }

    [Fact]
    public void FindProblems_MissingBattleSprite_NamesTheEnemy()
    {
        var goblin = _catalog.Enemies["goblin"] with { BattleSpriteFile = "Gone.png" };
        var catalog = _catalog with { Enemies = new Dictionary<string, EnemyDefinition>(_catalog.Enemies) { ["goblin"] = goblin } };

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(catalog, _artRoot));

        Assert.Equal($"battlers/Gone.png: not found in {_artRoot}. Named by enemy 'goblin'.", problem);
    }

    [Fact]
    public void FindProblems_MissingBattleSprite_NamesTheMember()
    {
        var jazog = _catalog.PartyMembers["jazog"] with { BattleSpriteFile = "Gone.png" };
        var catalog = _catalog with { PartyMembers = new Dictionary<string, PartyMemberDefinition>(_catalog.PartyMembers) { ["jazog"] = jazog } };

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(catalog, _artRoot));

        Assert.Equal($"battlers/Gone.png: not found in {_artRoot}. Named by party member 'jazog'.", problem);
    }

    [Fact]
    public void FindProblems_EnemyPortraitNameDiffersInCase_NamesTheEnemy()
    {
        var goblin = _catalog.Enemies["goblin"] with { PortraitFile = "goblin.png" };
        var catalog = _catalog with { Enemies = new Dictionary<string, EnemyDefinition>(_catalog.Enemies) { ["goblin"] = goblin } };

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(catalog, _artRoot));

        Assert.Equal("portraits/goblin.png: the file is named Goblin.png; names must match exactly. Named by enemy 'goblin'.", problem);
    }

    [Fact]
    public void FindProblems_MissingAttackEffect_NamesTheMember()
    {
        var jazog = _catalog.PartyMembers["jazog"] with { AttackEffectFile = "Gone.png" };
        var catalog = _catalog with { PartyMembers = new Dictionary<string, PartyMemberDefinition>(_catalog.PartyMembers) { ["jazog"] = jazog } };

        var problem = Assert.Single(ArtReferenceCheck.FindProblems(catalog, _artRoot));

        Assert.Equal($"effects/Gone.png: not found in {_artRoot}. Named by party member 'jazog'.", problem);
    }

    // Only the strip is in the art root, so the rest of the art is reported missing too; the test looks for its own line.
    [Fact]
    public void FindProblems_EffectStripWithPartialFrame_NamesTheFile()
    {
        var artRoot = Path.Combine(Path.GetTempPath(), $"jrpg-art-{Guid.NewGuid():N}");
        try
        {
            WritePngHeader(Path.Combine(artRoot, ArtFolders.Effects, "Partial.png"), width: 300, height: 128);
            var jazog = _catalog.PartyMembers["jazog"] with { AttackEffectFile = "Partial.png" };
            var catalog = _catalog with { PartyMembers = new Dictionary<string, PartyMemberDefinition>(_catalog.PartyMembers) { ["jazog"] = jazog } };

            var problems = ArtReferenceCheck.FindProblems(catalog, artRoot);

            Assert.Contains("effects/Partial.png: a 300x128 strip isn't a row of whole square frames.", problems);
        }
        finally
        {
            Directory.Delete(artRoot, recursive: true);
        }
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

    // Only the header: the art check reads a PNG's size from it (PngSize).
    private static void WritePngHeader(string path, int width, int height)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        byte[] header = [0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13, (byte)'I', (byte)'H', (byte)'D', (byte)'R', 0, 0, 0, 0, 0, 0, 0, 0];
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(16), width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(20), height);
        File.WriteAllBytes(path, header);
    }
}
