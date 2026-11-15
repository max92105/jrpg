using Jrpg.Core.Cinematics;
using Jrpg.Core.Content;
using Jrpg.Data.Art;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Art;

// A cinematic's stills (ADR 0046), on pictures the tests write into an art root of their own: only the stills are
// there, so the rest of the art is reported missing too, and each test looks for the stills' lines.
public sealed class ArtReferenceCheckCinematicTests : IDisposable
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private readonly string _artRoot = Path.Combine(Path.GetTempPath(), $"jrpg-art-{Guid.NewGuid():N}");

    public void Dispose() => Directory.Delete(_artRoot, recursive: true);

    [Fact]
    public void FindProblems_CinematicStillMissing_NamesCinematicAndStill()
    {
        WriteStill("test_cinematic/a.png");

        var problems = ArtReferenceCheck.FindProblems(WithStills("test_cinematic/a.png", "test_cinematic/b.png"), _artRoot);

        Assert.Equal(
            [$"cinematics/test_cinematic/b.png: not found in {_artRoot}. Named by cinematic 'test_cinematic' still 2."],
            StillProblems(problems));
    }

    [Fact]
    public void FindProblems_CinematicStillInItsFolder_IsFine()
    {
        WriteStill("test_cinematic/a.png");

        var problems = ArtReferenceCheck.FindProblems(WithStills("test_cinematic/a.png"), _artRoot);

        Assert.Empty(StillProblems(problems));
    }

    [Fact]
    public void FindProblems_CinematicStillOtherCase_NamesTheRealName()
    {
        WriteStill("test_cinematic/A.png");

        var problems = ArtReferenceCheck.FindProblems(WithStills("test_cinematic/a.png"), _artRoot);

        Assert.Equal(
            [
                "cinematics/test_cinematic/a.png: the file is named test_cinematic/A.png; names must match exactly. "
                + "Named by cinematic 'test_cinematic' still 1.",
            ],
            StillProblems(problems));
    }

    private static IEnumerable<string> StillProblems(IReadOnlyList<string> problems) =>
        problems.Where(problem => problem.Contains("Named by cinematic", StringComparison.Ordinal));

    private static ContentCatalog WithStills(params string[] imageFiles) =>
        _catalog with
        {
            Cinematics = new Dictionary<string, CinematicDefinition>
            {
                ["test_cinematic"] = new()
                {
                    Id = "test_cinematic",
                    Stills = [.. imageFiles.Select(file => new CinematicStill { ImageFile = file, Seconds = 4 })],
                },
            },
        };

    private void WriteStill(string imageFile) =>
        PngHeaderFile.Write(Path.Combine(_artRoot, ArtFolders.Cinematics, imageFile), width: 1920, height: 1080);
}
