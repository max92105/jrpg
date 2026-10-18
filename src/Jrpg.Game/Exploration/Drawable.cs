using Jrpg.Core.Maps;

namespace Jrpg.Game.Exploration;

/// <summary>
/// One thing the map scene draws and where it falls in the draw order. <c>Index</c> is the layer's or the placed
/// object's index in the map, and 0 for the player.
/// </summary>
internal readonly record struct Drawable(DrawOrderKey Key, DrawableKind Kind, int Index);
