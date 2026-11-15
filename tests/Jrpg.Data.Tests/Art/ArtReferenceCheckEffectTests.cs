using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Art;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Art;

// The strips and swings effect-strips.json lists (ADR 0048), on images the tests write into an art root of their own:
// only they are there, so the rest of the art is reported missing too, and each test looks for its own lines. The strip
// is four 8 x 8 frames.
public sealed class ArtReferenceCheckEffectTests : IDisposable
{
    private const string Strip = "Burst.png";
    private const string Swing = "Sweep.png";

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
    public void Check_ListedStripMissing_NamesIt()
    {
        var problems = ArtReferenceCheck.FindProblems(Listing(Record(hitFrame: 0)), _artRoot);

        Assert.Contains(problems, problem => problem.StartsWith($"effects/{Strip}: not found", StringComparison.Ordinal)
            && problem.EndsWith($"Named by effect-strips.json '{Strip}'.", StringComparison.Ordinal));
    }

    [Fact]
    public void Check_ListedStripMissing_IsReportedOnce()
    {
        var problems = ArtReferenceCheck.FindProblems(Listing(Record(hitFrame: 9)), _artRoot);

        Assert.Single(problems, problem => problem.Contains(Strip, StringComparison.Ordinal));
    }

    [Fact]
    public void Check_HitFrameOnTheLastFrame_Passes()
    {
        WriteStrip(Strip, width: 32);

        var problems = ArtReferenceCheck.FindProblems(Listing(Record(hitFrame: 3)), _artRoot);

        Assert.DoesNotContain(problems, problem => problem.Contains(Strip, StringComparison.Ordinal));
    }

    [Fact]
    public void Check_HitFramePastTheLastFrame_NamesItsFrames()
    {
        WriteStrip(Strip, width: 32);

        var problems = ArtReferenceCheck.FindProblems(Listing(Record(hitFrame: 4)), _artRoot);

        Assert.Contains($"effects/{Strip}: hit frame 4 lies past its 4 frames (from 0). Named by effect-strips.json.", problems);
    }

    [Fact]
    public void Check_SwingMissing_NamesTheStripThatNamesIt()
    {
        WriteStrip(Strip, width: 32);

        var problems = ArtReferenceCheck.FindProblems(Listing(Record(hitFrame: 0) with { SwingFile = Swing }), _artRoot);

        Assert.Contains(problems, problem => problem.StartsWith($"effects/{Swing}: not found", StringComparison.Ordinal)
            && problem.EndsWith($"Named by effect-strips.json '{Strip}' swing.", StringComparison.Ordinal));
    }

    [Fact]
    public void Check_SwingNotSquareFrames_NamesIt()
    {
        WriteStrip(Strip, width: 32);
        WriteStrip(Swing, width: 30);

        var problems = ArtReferenceCheck.FindProblems(Listing(Record(hitFrame: 0) with { SwingFile = Swing }), _artRoot);

        Assert.Contains($"effects/{Swing}: a 30x8 strip isn't a row of whole square frames.", problems);
    }

    private static EffectStripDefinition Record(int hitFrame) =>
        new() { File = Strip, FrameMilliseconds = 80, HitFrame = hitFrame };

    private static ContentCatalog Listing(EffectStripDefinition strip) =>
        _catalog with { EffectStrips = new Dictionary<string, EffectStripDefinition> { [strip.File] = strip } };

    private void WriteStrip(string file, int width) =>
        PngHeaderFile.Write(Path.Combine(_artRoot, ArtFolders.Effects, file), width, height: 8);
}
