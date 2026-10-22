namespace Jrpg.Editor.Tiles;

/// <summary>What an import copied and what it refused.</summary>
/// <param name="Imported">The file names copied into the tilesheet art, in the order chosen; replaced ones included.</param>
/// <param name="Refusals">Why each refused file wasn't copied, naming it.</param>
internal sealed record SheetImportResult(IReadOnlyList<string> Imported, IReadOnlyList<string> Refusals)
{
    /// <summary>The image to show after the import; null when nothing was copied.</summary>
    public string? LastImported => Imported.Count > 0 ? Imported[^1] : null;
}
