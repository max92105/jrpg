using Jrpg.Data.Content;

namespace Jrpg.ContentConverter.Tests;

public sealed class LegacyContentConversionTests : IDisposable
{
    private readonly string _contentRoot = Path.Combine(Path.GetTempPath(), $"jrpg-converter-{Guid.NewGuid():N}");

    public LegacyContentConversionTests()
    {
        Directory.CreateDirectory(_contentRoot);
    }

    public void Dispose() => Directory.Delete(_contentRoot, recursive: true);

    [Fact]
    public void Convert_RepositoryAssets_WritesTilesAndReport()
    {
        var report = LegacyContentConversion.Convert(TestPaths.LegacyAssets, _contentRoot);

        Assert.True(File.Exists(Path.Combine(_contentRoot, "generated", "tiles.json")));
        Assert.Equal(report.Lines, File.ReadAllLines(Path.Combine(_contentRoot, "generated", "conversion-report.txt")));
        Assert.Equal(["Tiles/Tileset.json: converted 283 tiles."], report.Lines);
    }

    [Fact]
    public void Convert_ExistingGeneratedFile_IsReplaced()
    {
        var staleFile = Path.Combine(_contentRoot, "generated", "maps", "removed_map.json");
        Directory.CreateDirectory(Path.GetDirectoryName(staleFile)!);
        File.WriteAllText(staleFile, "{}");

        LegacyContentConversion.Convert(TestPaths.LegacyAssets, _contentRoot);

        Assert.False(File.Exists(staleFile));
        Assert.False(Directory.Exists(Path.GetDirectoryName(staleFile)));
    }

    [Fact]
    public void Convert_FileOutsideGenerated_IsUntouched()
    {
        var handWritten = Path.Combine(_contentRoot, "maps", "start.json");
        Directory.CreateDirectory(Path.GetDirectoryName(handWritten)!);
        File.WriteAllText(handWritten, "hand-written");

        LegacyContentConversion.Convert(TestPaths.LegacyAssets, _contentRoot);

        Assert.Equal("hand-written", File.ReadAllText(handWritten));
    }

    [Fact]
    public void Convert_LegacyFieldTheConverterDoesNotKnow_ThrowsNamingFileAndPath()
    {
        var assetsRoot = Path.Combine(_contentRoot, "assets");
        Directory.CreateDirectory(Path.Combine(assetsRoot, "Tiles"));
        File.WriteAllText(
            Path.Combine(assetsRoot, "Tiles", "Tileset.json"),
            """{ "1": { "Id": 1, "Name": "Grass", "IsWalkable": true, "FrameDuration": 0.0, "Layer": 2 } }""");

        var exception = Assert.Throws<ConversionException>(() => LegacyContentConversion.Convert(assetsRoot, _contentRoot));

        Assert.StartsWith("Tiles/Tileset.json at $.1.Layer:", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Convert_DuplicateTileIdsAfterConversion_ThrowsAndWritesNothing()
    {
        var assetsRoot = Path.Combine(_contentRoot, "assets");
        Directory.CreateDirectory(Path.Combine(assetsRoot, "Tiles"));
        File.WriteAllText(
            Path.Combine(assetsRoot, "Tiles", "Tileset.json"),
            """{ "1": { "Id": 1, "Name": "A", "IsWalkable": true, "FrameDuration": 0.0 }, "01": { "Id": 1, "Name": "B", "IsWalkable": true, "FrameDuration": 0.0 } }""");

        var exception = Assert.Throws<ContentValidationException>(() => LegacyContentConversion.Convert(assetsRoot, _contentRoot));

        Assert.Equal("generated/tiles.json: tile id '1' is used more than once.", exception.Message);
        Assert.False(Directory.Exists(Path.Combine(_contentRoot, "generated")));
    }

    [Fact]
    public void Convert_MissingAssets_ThrowsAndKeepsExistingGeneratedContent()
    {
        var generatedTiles = Path.Combine(_contentRoot, "generated", "tiles.json");
        Directory.CreateDirectory(Path.GetDirectoryName(generatedTiles)!);
        File.WriteAllText(generatedTiles, "last good run");

        var exception = Assert.Throws<ConversionException>(
            () => LegacyContentConversion.Convert(Path.Combine(_contentRoot, "no-assets-here"), _contentRoot));

        Assert.StartsWith("Tiles/Tileset.json:", exception.Message, StringComparison.Ordinal);
        Assert.Equal("last good run", File.ReadAllText(generatedTiles));
    }
}
