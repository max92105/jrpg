using System.Windows;
using Jrpg.Data.Art;
using Microsoft.Win32;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The file dialog of an art folder's Import... (the Character sprites, Enemies and Effect strips pages): several
/// images at once, GIFs offered only where <see cref="ArtImageImport.TakesGifs"/> converts them.
/// </summary>
internal static class ArtImageFileDialog
{
    /// <param name="owner">The window hosting the page, which owns the file dialog.</param>
    /// <param name="title">The file dialog's title.</param>
    /// <param name="artFolder">The folder imported into (<see cref="ArtFolders"/>).</param>
    /// <returns>The chosen files in order; null on Cancel.</returns>
    public static string[]? Choose(Window owner, string title, string artFolder)
    {
        var dialog = new OpenFileDialog
        {
            Title = title,
            Filter = ArtImageImport.TakesGifs(artFolder) ? "Images (*.png;*.gif)|*.png;*.gif" : "PNG images (*.png)|*.png",
            Multiselect = true,
        };
        return dialog.ShowDialog(owner) == true ? dialog.FileNames : null;
    }
}
