using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>One color of a horizontal gradient, as a WPF <c>GradientStop</c>.</summary>
/// <param name="Offset">0 at the left edge to 1 at the right.</param>
/// <param name="Color">Premultiplied, like every texture the game draws.</param>
internal readonly record struct GradientStop(float Offset, Color Color);
