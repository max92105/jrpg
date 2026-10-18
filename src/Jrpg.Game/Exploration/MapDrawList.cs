using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Game.Exploration;

/// <summary>
/// The map's tile layers, placed objects and the player in the order Core's <see cref="DrawOrderKey"/> gives, so the
/// game draws what the editor will show. Rebuilding reuses its storage: nothing is allocated once it has grown.
/// </summary>
internal sealed class MapDrawList
{
    // The sprite is one cell tall and centered on the player's position, so the feet are half a cell below it (legacy MapRenderer).
    private const double PlayerFeetBelowPosition = 0.5;

    private static readonly IComparer<Drawable> _byDrawOrder = Comparer<Drawable>.Create((left, right) => left.Key.CompareTo(right.Key));

    private readonly List<Drawable> _drawables = [];

    public IReadOnlyList<Drawable> Drawables => _drawables;

    /// <param name="layout">The map whose layers and placed objects are drawn.</param>
    /// <param name="objects">Object definitions by id, for each placed object's height.</param>
    /// <param name="playerPosition">The player's position, in cells.</param>
    public void Rebuild(MapLayout layout, IReadOnlyDictionary<string, ObjectDefinition> objects, Point2D playerPosition)
    {
        _drawables.Clear();
        for (var index = 0; index < layout.Layers.Count; index++)
        {
            _drawables.Add(new Drawable(DrawOrderKey.ForTile(layout.Layers[index].Z, index), DrawableKind.Layer, index));
        }

        for (var index = 0; index < layout.Objects.Count; index++)
        {
            // An object sorts as a whole by its bottom edge, so overlapping objects never interleave their tiles.
            var placed = layout.Objects[index];
            var baseY = placed.Position.Y + objects[placed.ObjectId].Height;
            _drawables.Add(new Drawable(DrawOrderKey.ForSprite(placed.Z, baseY, index), DrawableKind.Object, index));
        }

        // After every object in insertion order, so a tie with an object's base never depends on the sort.
        var playerKey = DrawOrderKey.ForSprite(MapLayers.PlayerZ, playerPosition.Y + PlayerFeetBelowPosition, layout.Objects.Count);
        _drawables.Add(new Drawable(playerKey, DrawableKind.Player, 0));
        _drawables.Sort(_byDrawOrder);
    }
}
