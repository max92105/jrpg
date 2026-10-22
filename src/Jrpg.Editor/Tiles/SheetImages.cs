namespace Jrpg.Editor.Tiles;

/// <summary>The images in the tilesheet folder, as the browser and the Cut panel need them, so neither reads a file.</summary>
/// <param name="ListFiles">Every image's file name, in the order the sheet box lists them; read again after an import.</param>
/// <param name="SizeOf">An image's size in pixels, by its file name.</param>
internal sealed record SheetImages(Func<IReadOnlyList<string>> ListFiles, Func<string, (int Width, int Height)> SizeOf);
