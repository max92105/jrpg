using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;
using Jrpg.Data.Art;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Art;

// A field sprite must hold the frames its own record needs (ADR 0043), on sprites the tests write into an art root of
// their own: only the sprite is there, so the rest of the art is reported missing too, and each test looks for its own
// line. The records come from the frozen test world: 48 px frames, 8 walk frames on rows 0 to 3.
public sealed class ArtReferenceCheckFieldSpriteTests : IDisposable
{
    private const string Sprite = "Sheet.png";

    private static readonly string _testWorld = Path.Combine(AppContext.BaseDirectory, "test-world");
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(_testWorld);

    private readonly string _artRoot = Path.Combine(Path.GetTempPath(), $"jrpg-art-{Guid.NewGuid():N}");

    public void Dispose() => Directory.Delete(_artRoot, recursive: true);

    [Fact]
    public void FindProblems_SheetHoldingItsFrames_NoProblem()
    {
        PngHeaderFile.Write(Path.Combine(_artRoot, ArtFolders.FieldSprites, Sprite), width: 128, height: 224);

        var problems = ArtReferenceCheck.FindProblems(NewGameWith(BoughtSheet(frames: 4)), _artRoot);

        Assert.DoesNotContain(problems, problem => problem.Contains(Sprite, StringComparison.Ordinal));
    }

    [Fact]
    public void FindProblems_SheetTooNarrowForItsWalk_NamesFileAndFrames()
    {
        PngHeaderFile.Write(Path.Combine(_artRoot, ArtFolders.FieldSprites, Sprite), width: 96, height: 224);

        var problems = ArtReferenceCheck.FindProblems(NewGameWith(BoughtSheet(frames: 4)), _artRoot);

        Assert.Contains(
            $"field-sprites/{Sprite}: "
            + "a 96x224 sprite doesn't hold 4 x 4 frames of 32 x 32 px (128x128). Named by the new game.",
            problems);
    }

    [Fact]
    public void FindProblems_SheetTooShortForItsRows_NamesTheObject()
    {
        PngHeaderFile.Write(Path.Combine(_artRoot, ArtFolders.FieldSprites, Sprite), width: 384, height: 144);
        var villager = _catalog.Objects["villager"] with { FieldSpriteFile = Sprite };
        var catalog = _catalog with
        {
            Objects = new Dictionary<string, ObjectDefinition>(_catalog.Objects) { ["villager"] = villager },
            FieldSprites = WithSheet(_catalog.FieldSprites["npc_placeholder.png"] with { File = Sprite }),
        };

        var problems = ArtReferenceCheck.FindProblems(catalog, _artRoot);

        Assert.Contains(
            $"field-sprites/{Sprite}: "
            + "a 384x144 sprite doesn't hold 8 x 4 frames of 48 x 48 px (384x192). Named by object 'villager'.",
            problems);
    }

    // The player's sheet is a 48 px, 8 x 4 record (decision 561): one cell of it is refused as before FS2.
    [Fact]
    public void FindProblems_OneFrameOfThePlayerSheet_NamesTheNeededSize()
    {
        PngHeaderFile.Write(Path.Combine(_artRoot, ArtFolders.FieldSprites, Sprite), width: 48, height: 48);

        var playerSheet = _catalog.FieldSprites["player.png"] with { File = Sprite };

        var problems = ArtReferenceCheck.FindProblems(NewGameWith(playerSheet), _artRoot);

        Assert.Contains(
            $"field-sprites/{Sprite}: "
            + "a 48x48 sprite doesn't hold 8 x 4 frames of 48 x 48 px (384x192). Named by the new game.",
            problems);
    }

    // A frame count near the int limit is refused and said whole, never wrapped back inside the image.
    [Fact]
    public void FindProblems_FrameCountNearIntLimit_NoOverflow()
    {
        PngHeaderFile.Write(Path.Combine(_artRoot, ArtFolders.FieldSprites, Sprite), width: 384, height: 192);

        var problems = ArtReferenceCheck.FindProblems(NewGameWith(BoughtSheet(frames: int.MaxValue)), _artRoot);

        Assert.Contains(
            $"field-sprites/{Sprite}: "
            + "a 384x192 sprite doesn't hold 2147483647 x 4 frames of 32 x 32 px (68719476704x128). Named by the new game.",
            problems);
    }

    [Fact]
    public void FindProblems_RowNearIntLimit_NoOverflow()
    {
        PngHeaderFile.Write(Path.Combine(_artRoot, ArtFolders.FieldSprites, Sprite), width: 384, height: 192);
        var player = _catalog.FieldSprites["player.png"];
        var sheet = player with { File = Sprite, Walk = player.Walk with { Rows = player.Walk.Rows with { Up = int.MaxValue } } };

        var problems = ArtReferenceCheck.FindProblems(NewGameWith(sheet), _artRoot);

        Assert.Contains(
            $"field-sprites/{Sprite}: "
            + "a 384x192 sprite doesn't hold 8 x 2147483648 frames of 48 x 48 px (384x103079215104). Named by the new game.",
            problems);
    }

    private static ContentCatalog NewGameWith(FieldSpriteSheet sheet) =>
        _catalog with { NewGame = _catalog.NewGame with { FieldSpriteFile = sheet.File }, FieldSprites = WithSheet(sheet) };

    private static Dictionary<string, FieldSpriteSheet> WithSheet(FieldSpriteSheet sheet) =>
        new(_catalog.FieldSprites, StringComparer.Ordinal) { [sheet.File] = sheet };

    // A bought sheet's shape: 32 px frames, a facing per row from 0 to 3.
    private static FieldSpriteSheet BoughtSheet(int frames) => new()
    {
        File = Sprite,
        FrameWidth = 32,
        FrameHeight = 32,
        Scale = 3,
        Walk = new FacingCycle
        {
            Rows = new FacingRows { Down = 0, Left = 1, Right = 2, Up = 3 },
            Frames = new TileAnimation { FrameCount = frames, FrameMilliseconds = 150 },
        },
    };
}
