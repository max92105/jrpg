using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.GifImport;
using Jrpg.Editor.Tests.Tiles;

namespace Jrpg.Editor.Tests.Catalogs;

/// <summary>
/// Import... of the Character sprites window and Enemies page: the test world's records over an empty content folder.
/// </summary>
public sealed class ArtImageImportTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"jrpg-art-import-{Guid.NewGuid():N}");
    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));
    private readonly List<string> _questions = [];
    private readonly ArtImageImport _import;

    public ArtImageImportTests() => _import = new ArtImageImport(new ContentWriter(_root), _draft);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public void Import_NewSprite_CopiesItUnderItsName()
    {
        var source = PngHeaderFile.Write(Path.Combine(_root, "source", "Villager.png"), 128, 224);

        var result = _import.Import(ArtFolders.FieldSprites, [source], GifImportOptions.Default, Ask(answer: true));

        Assert.Equal("Villager.png", result.LastImported);
        Assert.True(File.Exists(Art(ArtFolders.FieldSprites, "Villager.png")));
        Assert.Empty(_questions);
    }

    [Fact]
    public void Import_NewBattler_CopiesItUnderItsName()
    {
        var source = PngHeaderFile.Write(Path.Combine(_root, "source", "Wolf.png"), 96, 96);

        var result = _import.Import(ArtFolders.Battlers, [source], GifImportOptions.Default, Ask(answer: true));

        Assert.Equal("Wolf.png", result.LastImported);
        Assert.True(File.Exists(Art(ArtFolders.Battlers, "Wolf.png")));
    }

    [Fact]
    public void Import_ExistingFile_AsksBeforeReplacing()
    {
        var old = PngHeaderFile.Write(Art(ArtFolders.Portraits, "Wolf.png"), 64, 64);
        var oldBytes = File.ReadAllBytes(old);
        var source = PngHeaderFile.Write(Path.Combine(_root, "source", "Wolf.png"), 96, 96);

        var result = _import.Import(ArtFolders.Portraits, [source], GifImportOptions.Default, Ask(answer: false));

        Assert.Equal(["Wolf.png"], _questions);
        Assert.Null(result.LastImported);
        Assert.Equal(oldBytes, File.ReadAllBytes(old));
    }

    // The placeholder's record needs 8 x 4 frames of 48 px: one frame of it is refused with Data's message.
    [Fact]
    public void Import_SpriteTooSmallForItsRecord_ReportsDataMessage()
    {
        var source = PngHeaderFile.Write(Path.Combine(_root, "source", "npc_placeholder.png"), 48, 48);

        var result = _import.Import(ArtFolders.FieldSprites, [source], GifImportOptions.Default, Ask(answer: true));

        Assert.Null(result.LastImported);
        var refusal = Assert.Single(result.Refusals);
        Assert.StartsWith("npc_placeholder.png: a 48x48 sprite doesn't hold", refusal, StringComparison.Ordinal);
    }

    // An effect strip is whole square frames (decision 57): 100 x 32 isn't, and Data says so.
    [Fact]
    public void Import_EffectNotWholeFrames_ReportsDataMessage()
    {
        var source = PngHeaderFile.Write(Path.Combine(_root, "source", "Bite.png"), 100, 32);

        var result = _import.Import(ArtFolders.Effects, [source], GifImportOptions.Default, Ask(answer: true));

        Assert.Null(result.LastImported);
        Assert.Equal(["Bite.png: a 100x32 strip isn't a row of whole square frames."], result.Refusals);
        Assert.False(File.Exists(Art(ArtFolders.Effects, "Bite.png")));
    }

    private string Art(string folder, string fileName) => Path.Combine(ArtFolders.RootIn(_root), folder, fileName);

    private Func<string, bool> Ask(bool answer) => fileName =>
    {
        _questions.Add(fileName);
        return answer;
    };
}
