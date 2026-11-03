namespace Jrpg.Data.Tests.Content;

/// <summary>
/// A copy of a content root (the test world, ADR 0037, or the repository's) in a temporary folder, so a test can break
/// one file and prove the loader refuses it by name without touching the shared content. Deleted with the test.
/// </summary>
internal sealed class CopiedContentRoot : IDisposable
{
    /// <param name="purpose">Names the temporary folder.</param>
    /// <param name="sourceFolder">
    /// The content root copied, under the test output: the test world, or the repository's content.
    /// </param>
    public CopiedContentRoot(string purpose, string sourceFolder)
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"jrpg-{purpose}-{Guid.NewGuid():N}");
        var source = System.IO.Path.Combine(AppContext.BaseDirectory, sourceFolder);
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = System.IO.Path.Combine(Path, System.IO.Path.GetRelativePath(source, file));
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
    }

    /// <summary>The copy's folder, what <c>ContentCatalogLoader.Load</c> takes.</summary>
    public string Path { get; }

    public string PathOf(string fileName) => System.IO.Path.Combine(Path, fileName);

    /// <summary>Rewrites one copied file with <paramref name="oldText"/> replaced, to break it on purpose.</summary>
    public void Replace(string fileName, string oldText, string newText) =>
        File.WriteAllText(PathOf(fileName), File.ReadAllText(PathOf(fileName)).Replace(oldText, newText, StringComparison.Ordinal));

    public void Dispose() => Directory.Delete(Path, recursive: true);
}
