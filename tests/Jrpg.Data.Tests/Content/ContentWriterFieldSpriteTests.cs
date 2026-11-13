using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

/// <summary>field-sprites.json is saved with the other catalogs, checked as the loader checks it (ADR 0043).</summary>
public sealed class ContentWriterFieldSpriteTests : IDisposable
{
    private const string Placeholder = "npc_placeholder.png";

    private readonly CopiedContentRoot _content = new("writer-sprites", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterFieldSpriteTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteMapCatalogs_ChangedSheet_WritesTheFieldSpritesFile()
    {
        _writer.WriteMapCatalogs(WithPlaceholder(sheet => sheet with { Scale = 2 }));

        Assert.Equal(2, ContentCatalogLoader.Load(_content.Path).FieldSprites[Placeholder].Scale);
    }

    [Fact]
    public void WriteMapCatalogs_SheetBreakingARule_ThrowsAndWritesNothing()
    {
        var before = File.ReadAllBytes(_content.PathOf("field-sprites.json"));

        var broken = WithPlaceholder(sheet => sheet with { Scale = 0 });

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(broken));

        Assert.StartsWith(
            "field-sprites.json: field sprite 'npc_placeholder.png': scale", exception.Message, StringComparison.Ordinal);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf("field-sprites.json")));
    }

    [Fact]
    public void WriteMapCatalogs_ObjectNamingASpriteWithoutRecord_ThrowsNamingTheObject()
    {
        var villager = _catalog.Objects["villager"] with { FieldSpriteFile = "ghost.png" };
        var objects = new Dictionary<string, ObjectDefinition>(_catalog.Objects) { ["villager"] = villager };
        var catalog = _catalog with { Objects = objects };

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(catalog));

        Assert.Equal(
            "field-sprites.json: object 'villager' names field sprite 'ghost.png', which has no record.", exception.Message);
    }

    private ContentCatalog WithPlaceholder(Func<FieldSpriteSheet, FieldSpriteSheet> change)
    {
        var sheets = new Dictionary<string, FieldSpriteSheet>(_catalog.FieldSprites, StringComparer.Ordinal);
        sheets[Placeholder] = change(sheets[Placeholder]);
        return _catalog with { FieldSprites = sheets };
    }
}
