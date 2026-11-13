using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Sprites;
using Jrpg.Editor.Tests.Tiles;

namespace Jrpg.Editor.Tests.Sprites;

/// <summary>Import... in the Character sprites window: the test world's records over an empty content folder.</summary>
public sealed class FieldSpriteImportTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"jrpg-sprite-import-{Guid.NewGuid():N}");
    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));
    private readonly List<string> _questions = [];
    private readonly FieldSpriteImport _import;

    public FieldSpriteImportTests() => _import = new FieldSpriteImport(new ContentWriter(_root), _draft);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public void Import_NewImage_CopiesItUnderItsName()
    {
        var source = PngHeaderFile.Write(Path.Combine(_root, "source", "Villager.png"), 128, 224);

        var result = _import.Import([source], Ask(answer: true));

        Assert.Equal("Villager.png", result.LastImported);
        Assert.True(File.Exists(SpriteArt("Villager.png")));
        Assert.Empty(_questions);
    }

    [Fact]
    public void Import_ExistingFile_AsksBeforeReplacing()
    {
        var old = PngHeaderFile.Write(SpriteArt("Villager.png"), 128, 224);
        var oldBytes = File.ReadAllBytes(old);
        var source = PngHeaderFile.Write(Path.Combine(_root, "source", "Villager.png"), 128, 256);

        var result = _import.Import([source], Ask(answer: false));

        Assert.Equal(["Villager.png"], _questions);
        Assert.Null(result.LastImported);
        Assert.Equal(oldBytes, File.ReadAllBytes(old));
    }

    // The placeholder's record needs 8 x 4 frames of 48 px: one frame of it is refused with Data's message.
    [Fact]
    public void Import_Refused_ReportsDataMessage()
    {
        var source = PngHeaderFile.Write(Path.Combine(_root, "source", "npc_placeholder.png"), 48, 48);

        var result = _import.Import([source], Ask(answer: true));

        Assert.Null(result.LastImported);
        var refusal = Assert.Single(result.Refusals);
        Assert.StartsWith("npc_placeholder.png: a 48x48 sprite doesn't hold", refusal, StringComparison.Ordinal);
    }

    private string SpriteArt(string fileName) => Path.Combine(ArtFolders.RootIn(_root), ArtFolders.FieldSprites, fileName);

    private Func<string, bool> Ask(bool answer) => fileName =>
    {
        _questions.Add(fileName);
        return answer;
    };
}
