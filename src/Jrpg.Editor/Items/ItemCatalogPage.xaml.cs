using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Items;

/// <summary>
/// The Items page (decisions 877, 879, 890): every item on the left with its Add, Duplicate and Remove buttons, the
/// chosen item's general fields on the right, its icon shown beside the box. Its host shows the problem line and keeps
/// or drops the whole session.
/// </summary>
internal sealed partial class ItemCatalogPage : UserControl
{
    /// <summary>
    /// The icon's picture: the game's 32 px icon at twice its size. A double, as the XAML sets a <c>Width</c> from it
    /// and WPF refuses an int there, failing as the page loads.
    /// </summary>
    public const double IconPictureSize = 64;

    /// <param name="items">The page's state, from <see cref="ItemEditingViewModel.Start"/>.</param>
    /// <param name="loadIcon">Decodes an image of the icons folder by its name there; null when it can't.</param>
    public ItemCatalogPage(ItemCatalogViewModel items, Func<string, BitmapSource?> loadIcon)
    {
        Resources.Add("PictureOf", new ArtPictureConverter(loadIcon));
        InitializeComponent();
        DataContext = items;
    }
}
