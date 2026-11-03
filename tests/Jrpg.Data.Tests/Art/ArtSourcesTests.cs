namespace Jrpg.Data.Tests.Art;

/// <summary>
/// The art sources manifest, the files under the repository's <c>content/art/</c> and the credits stay in step (ADR
/// 0038): a sheet imported without its line fails here (decision 448).
/// </summary>
public sealed class ArtSourcesTests
{
    // CC0, CC-BY, OFL for fonts and a bought pack's own terms; never NC or SA (decision 427, extended to art).
    private static readonly string[] _allowedLicenses =
        ["CC0-1.0", "CC-BY-3.0", "CC-BY-4.0", "OFL-1.1", "LicenseRef-ManaSeed", "LicenseRef-ElvGames", "LicenseRef-Unrecorded", "LicenseRef-Generated"];

    private static readonly string[] _placeholderOnlyLicenses = ["LicenseRef-Unrecorded", "LicenseRef-Generated"];

    private static readonly string[] _artExtensions = [".png", ".ttf"];

    private static readonly ArtSourcesManifest _manifest = ArtSourcesManifest.Read();

    // Every file under the art folder, by its exact relative path with forward slashes, as the manifest writes them.
    private static readonly IReadOnlyList<string> _filesOnDisk =
        [.. Directory.EnumerateFiles(ArtSourcesManifest.RepositoryArtRoot, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(ArtSourcesManifest.RepositoryArtRoot, path).Replace('\\', '/'))
            .Order(StringComparer.Ordinal)];

    [Fact]
    public void Manifest_EveryImageAndFont_HasAnEntry()
    {
        var art = _filesOnDisk.Where(path => _artExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase));
        var listed = _manifest.Files.Select(file => file.Path).ToHashSet(StringComparer.Ordinal);

        Assert.All(art, path => Assert.True(listed.Contains(path), $"{path} has no entry in art-sources.json."));
    }

    [Fact]
    public void Manifest_EveryEntry_NamesAFile()
    {
        // Exact case: Windows would find a wrongly cased name, the game on another system wouldn't.
        var onDisk = _filesOnDisk.ToHashSet(StringComparer.Ordinal);

        Assert.All(_manifest.Files, file => Assert.True(onDisk.Contains(file.Path), $"{file.Path} is listed but not in content/art, by that exact name."));
    }

    [Fact]
    public void Manifest_EveryEntry_NamesAKnownSource()
    {
        var sources = _manifest.Sources.Select(source => source.Id).ToHashSet(StringComparer.Ordinal);

        Assert.All(_manifest.Files, file => Assert.True(sources.Contains(file.SourceId), $"{file.Path} names the unknown source '{file.SourceId}'."));
    }

    [Fact]
    public void ArtFolder_EveryOtherFile_IsALicenseTextOrTheManifest()
    {
        // A pack's readme, sample map or Tiled sheet copied in by mistake fails here (decision 439).
        var allowed = _manifest.Sources.Select(source => source.LicenseFile).OfType<string>().Append("art-sources.json").Append("CREDITS.md").ToHashSet(StringComparer.Ordinal);
        var others = _filesOnDisk.Where(path => !_artExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase));

        Assert.All(others, path => Assert.True(allowed.Contains(path), $"{path} is neither art, a source's license text, the manifest nor the credits."));
    }

    [Fact]
    public void Manifest_EveryLicense_IsAllowed()
    {
        Assert.All(_manifest.Sources, source => Assert.Contains(source.License, _allowedLicenses));
    }

    [Fact]
    public void Manifest_UnrecordedOrGeneratedLicense_OnlyOnPlaceholderSources()
    {
        var placeholderLicensed = _manifest.Sources.Where(source => _placeholderOnlyLicenses.Contains(source.License, StringComparer.Ordinal));

        Assert.All(placeholderLicensed, source => Assert.True(source.IsPlaceholder, $"Source '{source.Id}' is {source.License} but not a placeholder."));
    }

    [Fact]
    public void Manifest_EveryFile_IsListedOnce()
    {
        // A file replaced under its old name and given a new line, with the placeholder line left in, would have two
        // sources.
        var listedTwice = _manifest.Files.GroupBy(file => file.Path, StringComparer.Ordinal).Where(group => group.Count() > 1).Select(group => group.Key);

        Assert.Empty(listedTwice);
    }

    [Fact]
    public void Credits_EverySource_IsListedWithItsAuthor()
    {
        // Name and author on one line: a section deleted while a longer name still holds the shorter one is caught.
        var lines = File.ReadAllLines(Path.Combine(ArtSourcesManifest.RepositoryArtRoot, "CREDITS.md"));

        Assert.All(_manifest.Sources, source => Assert.True(
            lines.Any(line => line.Contains(source.Name, StringComparison.Ordinal) && line.Contains(source.Author, StringComparison.Ordinal)),
            $"CREDITS.md has no line naming '{source.Name}' with its author '{source.Author}'."));
    }
}
