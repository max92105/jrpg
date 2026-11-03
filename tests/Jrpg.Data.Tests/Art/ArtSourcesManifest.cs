using System.Reflection;
using System.Text.Json;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Art;

/// <summary>
/// <c>content/art/art-sources.json</c> as the tests read it (ADR 0038): the game never loads it, so this is the only
/// reader. Read from the repository's folder, stamped into this assembly as <c>ContentRoot</c>, never from the test
/// output's copy, which keeps a deleted file.
/// </summary>
internal sealed record ArtSourcesManifest(IReadOnlyList<ArtSource> Sources, IReadOnlyList<ArtFile> Files)
{
    public static string RepositoryArtRoot { get; } = Path.Combine(RepositoryContentRoot(), "art");

    // The content loaders' strict options: a missing or misspelt field fails here, never reads as a default.
    public static ArtSourcesManifest Read() =>
        JsonSerializer.Deserialize<ArtSourcesManifest>(File.ReadAllText(Path.Combine(RepositoryArtRoot, "art-sources.json")), ContentJsonOptions.Strict)
        ?? throw new InvalidOperationException("art-sources.json holds null.");

    // Looked up here rather than through ContentRootLocator, which would quietly fall back to the stale output copy.
    private static string RepositoryContentRoot() =>
        typeof(ArtSourcesManifest).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().SingleOrDefault(metadata => metadata.Key == "ContentRoot")?.Value
        ?? throw new InvalidOperationException("The test assembly has no ContentRoot metadata.");
}

// Init properties as the content records declare them: the strict options require every constructor parameter, and the
// url and license file are optional.
internal sealed record ArtSource
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Author { get; init; }

    public string? Url { get; init; }

    public required string License { get; init; }

    public string? LicenseFile { get; init; }

    public required bool IsPlaceholder { get; init; }
}

internal sealed record ArtFile
{
    public required string Path { get; init; }

    public required string SourceId { get; init; }
}
