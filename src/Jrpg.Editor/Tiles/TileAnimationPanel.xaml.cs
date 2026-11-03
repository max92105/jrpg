using System.Windows.Controls;
using System.Windows.Media;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// The Animate panel (decision 467): the boxes, the preview and the buttons over a
/// <see cref="TileAnimationViewModel"/>.
/// </summary>
internal sealed partial class TileAnimationPanel : UserControl
{
    public TileAnimationPanel() => InitializeComponent();

    /// <summary>Shows the preview's frame; the window calls it from its render tick when the frame changes.</summary>
    public void ShowPreview(ImageSource frame) => Preview.Source = frame;
}
