using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// The player on the current map; the scene a new game starts in. Part 3a shows the tile layers and the standing
/// player; walking, objects and events come in 3b.
/// </summary>
internal sealed class ExplorationScene : IScene
{
    // One frame of the legacy sprite sheet, in the image's pixels; equal to a screen cell only by coincidence (question 58).
    private const int FieldSpriteFramePixels = 48;

    private readonly MapLocation _location;
    private readonly MapLayout _layout;
    private readonly MapView _view;
    private readonly string _fieldSpriteFile;
    private readonly MapCamera _camera = new();

    public ExplorationScene(MapLocation location, ContentCatalog catalog, TileAtlas atlas)
    {
        _location = location;
        _layout = catalog.Maps[location.MapId].Layout;
        _view = new MapView(_layout, atlas);
        _fieldSpriteFile = catalog.NewGame.FieldSpriteFile;
        _camera.SnapTo(location.Position, _layout);
    }

    public void Update(TimeSpan delta, InputFrame input)
    {
    }

    /// <summary>Layers up to the player's draw first, then the player, then the layers above (<see cref="MapLayers"/>).</summary>
    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        var origin = _camera.Origin;
        var layers = _layout.Layers;
        var layerIndex = 0;
        for (; layerIndex < layers.Count && layers[layerIndex].Z <= MapLayers.PlayerZ; layerIndex++)
        {
            _view.DrawLayer(batch, textures, layers[layerIndex], origin);
        }

        DrawPlayer(batch, textures, origin);
        for (; layerIndex < layers.Count; layerIndex++)
        {
            _view.DrawLayer(batch, textures, layers[layerIndex], origin);
        }
    }

    // The sprite is one cell square, centered on the player's position (legacy MapRenderer); frame 0 until walking animates it.
    private void DrawPlayer(SpriteBatch batch, TextureLibrary textures, Vector2 origin)
    {
        const int size = MapView.CellPixels;
        var position = _location.Position;
        var left = (int)Math.Round((position.X * size) - (size / 2.0) - origin.X);
        var top = (int)Math.Round((position.Y * size) - (size / 2.0) - origin.Y);
        var source = new Rectangle(0, SpriteRowOf(_location.Facing) * FieldSpriteFramePixels, FieldSpriteFramePixels, FieldSpriteFramePixels);
        batch.Draw(textures.Get(ArtFolders.FieldSprites, _fieldSpriteFile), new Rectangle(left, top, size, size), source, Color.White);
    }

    // The legacy sheet's rows, top to bottom: down, left, right, up.
    private static int SpriteRowOf(Facing facing) => facing switch
    {
        Facing.Down => 0,
        Facing.Left => 1,
        Facing.Right => 2,
        Facing.Up => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, null),
    };
}
