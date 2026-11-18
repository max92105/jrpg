using System.Windows;
using Jrpg.Data.Art;
using Microsoft.Win32;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The file dialog of an art folder's Import... (the Character sprites, Enemies, Effect strips and Battle backgrounds
/// pages): several images at once, GIFs offered only where <see cref="ArtImageImport.TakesGifs"/> converts them; the
/// Party members page's one PNG for one box.
/// </summary>
internal static class ArtImageFileDialog
{
    private const string PngFilter = "PNG images (*.png)|*.png";

    /// <param name="owner">The window hosting the page, which owns the file dialog.</param>
    /// <param name="title">The file dialog's title.</param>
    /// <param name="artFolder">The folder imported into (<see cref="ArtFolders"/>).</param>
    /// <returns>The chosen files in order; null on Cancel.</returns>
    public static string[]? Choose(Window owner, string title, string artFolder)
    {
        var dialog = new OpenFileDialog
        {
            Title = title,
            Filter = ArtImageImport.TakesGifs(artFolder) ? "Images (*.png;*.gif)|*.png;*.gif" : PngFilter,
            Multiselect = true,
        };
        return dialog.ShowDialog(owner) == true ? dialog.FileNames : null;
    }

    /// <summary>
    /// One PNG for a box that takes no GIF strip even where its folder converts them: a party member's (decision 953).
    /// </summary>
    /// <param name="owner">The window hosting the page, which owns the file dialog.</param>
    /// <param name="title">The file dialog's title.</param>
    /// <returns>The chosen file; null on Cancel.</returns>
    public static string? ChoosePng(Window owner, string title)
    {
        var dialog = new OpenFileDialog { Title = title, Filter = PngFilter };
        return dialog.ShowDialog(owner) == true ? dialog.FileName : null;
    }
}
