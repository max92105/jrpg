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

    [Fact]
    public void WriteFieldSprites_RecordWithSequence_ReadsBackEqual()
    {
        var up = new FacingSequence { Cells = [new SpriteCell(2, 5), new SpriteCell(0, 5), new SpriteCell(2, 5)] };
        var catalog = WithPlaceholder(sheet => WithUp(sheet, up));

        _writer.WriteMapCatalogs(catalog);

        Assert.Equal(catalog.FieldSprites[Placeholder], ContentCatalogLoader.Load(_content.Path).FieldSprites[Placeholder]);
    }

    [Fact]
    public void WriteFieldSprites_RowRecord_WritesNoSequences()
    {
        _writer.WriteMapCatalogs(WithPlaceholder(sheet => sheet with { Scale = 2 }));

        Assert.DoesNotContain("sequences", File.ReadAllText(_content.PathOf("field-sprites.json")), StringComparison.Ordinal);
    }

    // A sequence for one facing writes that facing alone.
    [Fact]
    public void WriteFieldSprites_SequenceForOneFacing_LeavesTheOthersOut()
    {
        var up = new FacingSequence { Cells = [new SpriteCell(1, 1)] };
        _writer.WriteMapCatalogs(WithPlaceholder(sheet => WithUp(sheet, up)));

        var written = File.ReadAllText(_content.PathOf("field-sprites.json"));

        Assert.Contains("\"up\": {", written, StringComparison.Ordinal);
        Assert.DoesNotContain("\"down\": null", written, StringComparison.Ordinal);
    }

    private static FieldSpriteSheet WithUp(FieldSpriteSheet sheet, FacingSequence up) =>
        sheet with { Walk = sheet.Walk with { Sequences = new FacingSequences { Up = up } } };

    private ContentCatalog WithPlaceholder(Func<FieldSpriteSheet, FieldSpriteSheet> change)
    {
        var sheets = new Dictionary<string, FieldSpriteSheet>(_catalog.FieldSprites, StringComparer.Ordinal);
        sheets[Placeholder] = change(sheets[Placeholder]);
        return _catalog with { FieldSprites = sheets };
    }
}
