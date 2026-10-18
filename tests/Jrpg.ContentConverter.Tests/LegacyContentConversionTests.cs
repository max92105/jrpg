using Jrpg.Core.Events;
using Jrpg.Data.Content;

namespace Jrpg.ContentConverter.Tests;

public sealed class LegacyContentConversionTests : IDisposable
{
    private readonly string _contentRoot = Path.Combine(Path.GetTempPath(), $"jrpg-converter-{Guid.NewGuid():N}");

    public LegacyContentConversionTests()
    {
        // The converter checks event items against the hand-written item list.
        Directory.CreateDirectory(_contentRoot);
        File.Copy(Path.Combine(TestPaths.RepositoryContent, "items.json"), Path.Combine(_contentRoot, "items.json"));
    }

    public void Dispose() => Directory.Delete(_contentRoot, recursive: true);

    [Fact]
    public void Convert_RepositoryAssets_WritesTheReport()
    {
        var report = LegacyContentConversion.Convert(TestPaths.LegacyAssets, _contentRoot);

        Assert.Equal(report.Lines, File.ReadAllLines(Path.Combine(_contentRoot, "generated", "conversion-report.txt")));
        Assert.Equal("Tiles/Tileset.json: converted 283 tiles.", report.Lines[0]);
        Assert.Equal(8, report.Lines.Count(line => line.Contains("stale size", StringComparison.Ordinal)));
        Assert.Contains("Objects/Objects.json: converted 12 objects.", report.Lines);
        Assert.Equal("Maps/start.json: converted map 'start' (45x45, 2 layers, 55 objects, 3 object events, 2 cell events).", report.Lines[^1]);
    }

    [Fact]
    public void Convert_RepositoryAssets_ProducesLoadableContent()
    {
        CopyFolder(TestPaths.RepositoryContent, _contentRoot);

        LegacyContentConversion.Convert(TestPaths.LegacyAssets, _contentRoot);
        var catalog = ContentCatalogLoader.Load(_contentRoot);

        Assert.Equal(["forest", "interior_1", "start"], catalog.Maps.Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Convert_RepositoryAssets_ChestGivesPotionOnce()
    {
        CopyFolder(TestPaths.RepositoryContent, _contentRoot);

        LegacyContentConversion.Convert(TestPaths.LegacyAssets, _contentRoot);
        var chest = ContentCatalogLoader.Load(_contentRoot).Maps["start"].Layout.Objects.Single(placed => placed.ObjectId == "chest");

        var openChest = Assert.Single(chest.Events);
        Assert.True(openChest.IsOnce);
        Assert.Equal([new GiveItemStep("potion", 1), new SwapObjectStep("chest_opened")], openChest.Steps);
    }

    [Fact]
    public void Convert_ContentFolderWithoutItems_ThrowsNamingTheFile()
    {
        File.Delete(Path.Combine(_contentRoot, "items.json"));

        var exception = Assert.Throws<ConversionException>(() => LegacyContentConversion.Convert(TestPaths.LegacyAssets, _contentRoot));

        Assert.StartsWith("items.json: not found", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Convert_TwoMapFilesSameId_ThrowsNamingBothAndWritesNothing()
    {
        var assetsRoot = Path.Combine(_contentRoot, "assets");
        CopyFolder(TestPaths.LegacyAssets, assetsRoot);
        File.Copy(Path.Combine(assetsRoot, "Maps", "Interior1.json"), Path.Combine(assetsRoot, "Maps", "interior_1.json"));

        var exception = Assert.Throws<ConversionException>(() => LegacyContentConversion.Convert(assetsRoot, _contentRoot));

        Assert.Equal("Maps/Interior1.json and Maps/interior_1.json both become map id 'interior_1'.", exception.Message);
        Assert.False(Directory.Exists(Path.Combine(_contentRoot, "generated")));
    }

    // Catches a map edited in the legacy editor but never converted, and a converter change never re-run.
    [Fact]
    public void Convert_RepositoryAssets_MatchesCommittedGeneratedContent()
    {
        LegacyContentConversion.Convert(TestPaths.LegacyAssets, _contentRoot);

        var committedRoot = Path.Combine(TestPaths.RepositoryContent, "generated");
        var freshRoot = Path.Combine(_contentRoot, "generated");
        Assert.Equal(RelativeFiles(committedRoot), RelativeFiles(freshRoot));
        Assert.All(
            RelativeFiles(committedRoot),
            file => Assert.True(
                File.ReadAllBytes(Path.Combine(committedRoot, file)).SequenceEqual(File.ReadAllBytes(Path.Combine(freshRoot, file))),
                $"content/generated/{file} is stale: run the content converter."));
    }

    private static List<string> RelativeFiles(string root) =>
        [.. Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
            .Order(StringComparer.Ordinal)];

    private static void CopyFolder(string source, string destination)
    {
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite: true);
        }
    }

    [Fact]
    public void Convert_ExistingGeneratedFile_IsReplaced()
    {
        var staleFile = Path.Combine(_contentRoot, "generated", "maps", "removed_map.json");
        Directory.CreateDirectory(Path.GetDirectoryName(staleFile)!);
        File.WriteAllText(staleFile, "{}");

        LegacyContentConversion.Convert(TestPaths.LegacyAssets, _contentRoot);

        Assert.False(File.Exists(staleFile));
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
            """
            { "1": { "Id": 1, "Name": "A", "IsWalkable": true, "FrameDuration": 0.0, "Tilesheet": "City.png", "SheetX": 0, "SheetY": 0 },
              "01": { "Id": 1, "Name": "B", "IsWalkable": true, "FrameDuration": 0.0, "Tilesheet": "City.png", "SheetX": 1, "SheetY": 0 } }
            """);
        File.WriteAllText(Path.Combine(assetsRoot, "Tiles", "Tilesheets.json"), "[]");

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
