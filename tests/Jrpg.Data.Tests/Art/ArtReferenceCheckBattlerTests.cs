using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Art;

// An animated enemy's strip must split into its loop's frames (ADR 0047), on strips the tests write into an art root of
// their own: only the strip is there, so the rest of the art is reported missing too, and each test looks for its own
// line. The enemy is the test world's goblin, renamed and given the strip.
public sealed class ArtReferenceCheckBattlerTests : IDisposable
{
    private const string Strip = "TestStrip.png";

    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private readonly string _artRoot = Path.Combine(Path.GetTempPath(), $"jrpg-art-{Guid.NewGuid():N}");

    public void Dispose() => Directory.Delete(_artRoot, recursive: true);

    [Fact]
    public void FindProblems_StripOfWholeFrames_IsFine()
    {
        PngHeaderFile.Write(Path.Combine(_artRoot, ArtFolders.Battlers, Strip), width: 800, height: 100);

        var problems = ArtReferenceCheck.FindProblems(WithGnoll(EightFrames), _artRoot);

        Assert.DoesNotContain(problems, problem => problem.Contains(Strip, StringComparison.Ordinal));
    }

    [Fact]
    public void FindProblems_StripNotWholeFrames_NamesEnemyAndSize()
    {
        PngHeaderFile.Write(Path.Combine(_artRoot, ArtFolders.Battlers, Strip), width: 810, height: 100);

        var problems = ArtReferenceCheck.FindProblems(WithGnoll(EightFrames), _artRoot);

        Assert.Contains($"battlers/{Strip}: a 810x100 strip doesn't split into 8 frames. Named by enemy 'gnoll'.", problems);
    }

    [Fact]
    public void FindProblems_StillSpriteOddWidth_IsFine()
    {
        PngHeaderFile.Write(Path.Combine(_artRoot, ArtFolders.Battlers, Strip), width: 810, height: 100);

        var problems = ArtReferenceCheck.FindProblems(WithGnoll(null), _artRoot);

        Assert.DoesNotContain(problems, problem => problem.Contains(Strip, StringComparison.Ordinal));
    }

    private static TileAnimation EightFrames => new() { FrameCount = 8, FrameMilliseconds = 100 };

    private static ContentCatalog WithGnoll(TileAnimation? idle)
    {
        var gnoll = _catalog.Enemies["gnoll"] with { Id = "gnoll", BattleSpriteFile = Strip, IdleAnimation = idle };
        return _catalog with { Enemies = new Dictionary<string, EnemyDefinition>(_catalog.Enemies) { ["gnoll"] = gnoll } };
    }
}
