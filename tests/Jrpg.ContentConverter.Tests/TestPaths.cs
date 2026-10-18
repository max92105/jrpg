namespace Jrpg.ContentConverter.Tests;

internal static class TestPaths
{
    /// <summary>The repository's <c>Assets/</c> JSON files, copied next to the tests by the project file.</summary>
    public static string LegacyAssets { get; } = Path.Combine(AppContext.BaseDirectory, "legacy-assets");

    /// <summary>The repository's <c>content/</c> folder, copied next to the tests by the project file.</summary>
    public static string RepositoryContent { get; } = Path.Combine(AppContext.BaseDirectory, "content");
}
