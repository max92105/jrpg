using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// The legacy UI's colors and sizes in one place (developer decision 82), so screens look alike and change together.
/// </summary>
internal static class UiTheme
{
    /// <summary>Legacy <c>DialogueBox.xaml</c>'s line height, 16, doubled like the box.</summary>
    public const int MessageTextPixels = 32;

    /// <summary>
    /// Legacy <c>DialogueBox.xaml</c>, doubled like the box: <c>#AA000000</c> inside a 2 px white border with 10 px
    /// corners. Black stays black when premultiplied, so the fill is written as is.
    /// </summary>
    public static PanelStyle MessagePanel { get; } = new(CornerRadius: 20, BorderWidth: 4, Fill: new Color(0, 0, 0, 0xAA), Border: Color.White);

    public static Color MessageText => Color.White;
}
