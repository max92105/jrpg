using Jrpg.Core.Maps;

namespace Jrpg.Editor.Tiles;

/// <summary>What an import copied and what it refused.</summary>
/// <param name="Imported">The file names copied into the tilesheet art, in the order chosen; replaced ones included.</param>
/// <param name="Refusals">Why each refused file wasn't copied, naming it.</param>
internal sealed record SheetImportResult(IReadOnlyList<string> Imported, IReadOnlyList<string> Refusals)
{
    /// <summary>The loop each strip imported from a GIF carries, by its PNG name (ADR 0047).</summary>
    public IReadOnlyDictionary<string, TileAnimation> Animations { get; init; } = new Dictionary<string, TileAnimation>();

    /// <summary>
    /// What the status line says of an imported file beside its name, by name: a strip's frames and time.
    /// </summary>
    public IReadOnlyDictionary<string, string> Notes { get; init; } = new Dictionary<string, string>();

    /// <summary>The image to show after the import; null when nothing was copied.</summary>
    public string? LastImported => Imported.Count > 0 ? Imported[^1] : null;

    /// <summary>
    /// The status line: "Imported A.png, B.png (8 frames of 100 x 128 px, 100 ms). Refused: C.png: not a PNG image."
    /// </summary>
    public string Summary
    {
        get
        {
            var refusals = Refusals.Select(refusal => $"Refused: {refusal}");
            return Imported.Count == 0
                ? string.Join(" ", refusals)
                : string.Join(" ", refusals.Prepend($"Imported {string.Join(", ", Imported.Select(Noted))}."));
        }
    }

    private string Noted(string fileName) => Notes.TryGetValue(fileName, out var note) ? $"{fileName} ({note})" : fileName;
}
