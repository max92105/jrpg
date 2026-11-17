using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Art;

// A background's size (decision 895) and an animated background's grid (ADR 0059), on images the tests write into an
// art root of their own: only the image is there, so the rest of the art is reported missing too, and each test looks
// for its own line. The catalog is the test world's, its animated backgrounds replaced by the test's.
public sealed class ArtReferenceCheckBackgroundSizeTests : IDisposable
{
    private const string Sheet = "Waves.png";
    private const string RecordNamedBy = $"Named by animated-backgrounds.json '{Sheet}'.";

    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private readonly string _artRoot = Path.Combine(Path.GetTempPath(), $"jrpg-art-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_artRoot))
        {
            Directory.Delete(_artRoot, recursive: true);
        }
    }

    [Fact]
    public void FindProblems_RecordImageMissing_NamesTheRecord()
    {
        var problems = ArtReferenceCheck.FindProblems(WithRecord(FourFrames), _artRoot);

        Assert.Contains($"backgrounds/{Sheet}: not found in {_artRoot}. {RecordNamedBy}", problems);
    }

    [Fact]
    public void FindProblems_RecordImageHoldsItsGrid_NoProblem()
    {
        WriteBackground(Sheet, width: 1920, height: 1080);

        var problems = ArtReferenceCheck.FindProblems(WithRecord(FourFrames), _artRoot);

        Assert.DoesNotContain(problems, problem => problem.Contains(Sheet, StringComparison.Ordinal));
    }

    [Fact]
    public void FindProblems_RecordImageOneRowShort_NamesTheGrid()
    {
        WriteBackground(Sheet, width: 1920, height: 540);

        var problems = ArtReferenceCheck.FindProblems(WithRecord(FourFrames), _artRoot);

        Assert.Contains(
            $"backgrounds/{Sheet}: 4 frames of 960 x 540 px need 2 x 2; the image holds 2 x 1. {RecordNamedBy}", problems);
    }

    [Fact]
    public void FindProblems_RecordImageNarrowerThanAFrame_IsAProblem()
    {
        WriteBackground(Sheet, width: 959, height: 2160);

        var problems = ArtReferenceCheck.FindProblems(WithRecord(FourFrames), _artRoot);

        Assert.Contains(
            $"backgrounds/{Sheet}: 4 frames of 960 x 540 px need 1 x 4; the image holds 0 x 4. {RecordNamedBy}", problems);
    }

    [Fact]
    public void FindProblems_RecordWithNearMaxFrameCount_IsAProblemWithoutOverflow()
    {
        WriteBackground(Sheet, width: 1920, height: 1080);
        var record = FourFrames with { Animation = new TileAnimation { FrameCount = int.MaxValue, FrameMilliseconds = 200 } };

        var problems = ArtReferenceCheck.FindProblems(WithRecord(record), _artRoot);

        Assert.Contains(
            $"backgrounds/{Sheet}: 2147483647 frames of 960 x 540 px need 2 x 1073741824; the image holds 2 x 2. "
            + RecordNamedBy,
            problems);
    }

    // Too large wins over the grid: the game can't load the sheet at all.
    [Fact]
    public void FindProblems_RecordImageOver4096_SaysItIsTooLarge()
    {
        WriteBackground(Sheet, width: 4097, height: 540);

        var problems = ArtReferenceCheck.FindProblems(WithRecord(FourFrames), _artRoot);

        Assert.Contains(
            $"backgrounds/{Sheet}: a 4097 x 540 px image is larger than the game loads (4096 px a side). {RecordNamedBy}",
            problems);
    }

    [Fact]
    public void GridProblemOf_ImageExactlyHoldsTheGrid_IsNull()
    {
        var problem = ArtReferenceCheck.GridProblemOf(FourFrames, (1920, 1080));

        Assert.Null(problem);
    }

    // Three frames on two columns: the second row is half filled, and the width leaves a part frame at the right edge.
    [Fact]
    public void GridProblemOf_LastRowPartlyFilled_IsNull()
    {
        var record = FourFrames with { Animation = FourFrames.Animation with { FrameCount = 3 } };

        var problem = ArtReferenceCheck.GridProblemOf(record, (2000, 1080));

        Assert.Null(problem);
    }

    [Fact]
    public void GridProblemOf_ImageOneFrameWide_IsNull()
    {
        var problem = ArtReferenceCheck.GridProblemOf(FourFrames, (960, 2160));

        Assert.Null(problem);
    }

    [Fact]
    public void GridProblemOf_ImageOneColumnShortOnASingleRow_NamesTheGrid()
    {
        var problem = ArtReferenceCheck.GridProblemOf(FourFrames, (3839, 540));

        Assert.Equal("4 frames of 960 x 540 px need 3 x 2; the image holds 3 x 1", problem);
    }

    [Fact]
    public void FindProblems_MapBackgroundOver4096High_IsAProblem()
    {
        WriteBackground("Tall.png", width: 2000, height: 4097);

        var problems = ArtReferenceCheck.FindProblems(WithForestBackground("Tall.png"), _artRoot);

        Assert.Contains(
            "backgrounds/Tall.png: a 2000 x 4097 px image is larger than the game loads (4096 px a side). "
            + "Named by map 'forest'.",
            problems);
    }

    [Fact]
    public void FindProblems_MapBackgroundExactly4096_NoProblem()
    {
        WriteBackground("Tall.png", width: 4096, height: 4096);

        var problems = ArtReferenceCheck.FindProblems(WithForestBackground("Tall.png"), _artRoot);

        Assert.DoesNotContain(problems, problem => problem.Contains("Tall.png", StringComparison.Ordinal));
    }

    [Fact]
    public void FindProblems_EncounterBackgroundOver4096Wide_IsAProblem()
    {
        WriteBackground("Wide.png", width: 4097, height: 2000);
        var trio = _catalog.Encounters["goblin_trio"] with { BackgroundFile = "Wide.png" };
        var catalog = _catalog with
        {
            Encounters = new Dictionary<string, EncounterDefinition>(_catalog.Encounters) { ["goblin_trio"] = trio },
        };

        var problems = ArtReferenceCheck.FindProblems(catalog, _artRoot);

        Assert.Contains(
            "backgrounds/Wide.png: a 4097 x 2000 px image is larger than the game loads (4096 px a side). "
            + "Named by encounter 'goblin_trio'.",
            problems);
    }

    private static AnimatedBackgroundDefinition FourFrames => new()
    {
        File = Sheet,
        FrameWidth = 960,
        FrameHeight = 540,
        Animation = new TileAnimation { FrameCount = 4, FrameMilliseconds = 200 },
    };

    private static ContentCatalog WithRecord(AnimatedBackgroundDefinition record) =>
        _catalog with { AnimatedBackgrounds = new Dictionary<string, AnimatedBackgroundDefinition> { [record.File] = record } };

    private static ContentCatalog WithForestBackground(string file)
    {
        var forest = _catalog.Maps["forest"];
        var map = new MapDefinition(forest.Layout, forest.Settings with { BattleBackgroundFile = file });
        return _catalog with { Maps = new Dictionary<string, MapDefinition>(_catalog.Maps) { ["forest"] = map } };
    }

    private void WriteBackground(string file, int width, int height) =>
        PngHeaderFile.Write(Path.Combine(_artRoot, ArtFolders.Backgrounds, file), width, height);
}
