using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>A rounded panel's look, in virtual-screen pixels.</summary>
/// <param name="CornerRadius">Radius of the outer corners.</param>
/// <param name="BorderWidth">Width of the border inside the outer edge.</param>
/// <param name="Fill">Inside the border, with premultiplied alpha like every texture the game draws.</param>
/// <param name="Border">The border, premultiplied likewise.</param>
internal sealed record PanelStyle(int CornerRadius, int BorderWidth, Color Fill, Color Border);
