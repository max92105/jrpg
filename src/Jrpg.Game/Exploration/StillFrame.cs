namespace Jrpg.Game.Exploration;

/// <summary>A still as it shows this frame: its picture, how faded and how zoomed (decisions 617, 629).</summary>
/// <param name="ImageFile">The picture under the cinematics folder.</param>
/// <param name="Opacity">0 to 1: the picture and its subtitle fade with it.</param>
/// <param name="Scale">
/// The picture's size as a factor of its fitted size, about the screen's center; 1 is no zoom.
/// </param>
internal readonly record struct StillFrame(string ImageFile, float Opacity, float Scale);
