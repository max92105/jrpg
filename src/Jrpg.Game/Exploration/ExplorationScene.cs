using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// The player walking the current map; the scene a new game starts in. It shows the tile layers, the animated player and
/// objects in depth order, canopies and roofs over the player, with the camera easing after the player.
/// </summary>
internal sealed class ExplorationScene : IScene
{
    // One frame of the legacy sprite sheet, in the image's pixels; equal to a screen cell only by coincidence (question 58).
    private const int FieldSpriteFramePixels = 48;

    // Legacy GameManager: the camera catches up quickly while the player walks and settles gently once they stop.
    private const double WalkingCameraShare = 0.35;
    private const double StandingCameraShare = 0.1;

    private readonly ExplorationState _exploration;
    private readonly IReadOnlyDictionary<string, ObjectDefinition> _objects;
    private readonly MapLayout _layout;
    private readonly MapView _view;
    private readonly string _fieldSpriteFile;
    private readonly MapCamera _camera = new();
    private readonly MapDrawList _drawList = new();
    private readonly WalkAnimation _walkAnimation = new();
    private readonly CanopyTextures _canopies;

    public ExplorationScene(ExplorationState exploration, ContentCatalog catalog, TileAtlas atlas, CanopyTextures canopies)
    {
        _exploration = exploration;
        _objects = catalog.Objects;
        _canopies = canopies;
        _layout = catalog.Maps[exploration.Location.MapId].Layout;
        _view = new MapView(_layout, catalog.Objects, atlas);
        _fieldSpriteFile = catalog.NewGame.FieldSpriteFile;
        _camera.SnapTo(exploration.PlayerPosition, _layout);
        _drawList.Rebuild(_layout, _objects, exploration.PlayerPosition);
    }

    /// <summary>Walks while no event runs; until events are shown (M2), a started event leaves the player standing still.</summary>
    public void Update(TimeSpan delta, InputFrame input)
    {
        if (_exploration.IsEventRunning)
        {
            _walkAnimation.Stand();
            return;
        }

        _exploration.Walk(input.Move, delta);
        var isMoving = _exploration.IsPlayerMoving;
        if (isMoving)
        {
            _walkAnimation.Advance(delta);
        }
        else
        {
            _walkAnimation.Stand();
        }

        var position = _exploration.PlayerPosition;
        _camera.Follow(position, _layout, delta, isMoving ? WalkingCameraShare : StandingCameraShare);
        _drawList.Rebuild(_layout, _objects, position);
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        var origin = _camera.Origin;
        var drawables = _drawList.Drawables;
        for (var index = 0; index < drawables.Count; index++)
        {
            var drawable = drawables[index];
            switch (drawable.Kind)
            {
                case DrawableKind.Layer:
                    _view.DrawLayer(batch, textures, _layout.Layers[drawable.Index], origin);
                    break;
                case DrawableKind.Object:
                    _view.DrawObject(batch, textures, _layout.Objects[drawable.Index], origin);
                    break;
                case DrawableKind.Canopy:
                    var placed = _layout.Objects[drawable.Index];
                    _view.DrawCanopy(batch, _canopies.CanopyOf(placed.ObjectId, textures), placed, origin);
                    break;
                case DrawableKind.Player:
                    DrawPlayer(batch, textures, origin);
                    break;
            }
        }
    }

    // The sprite is one cell square, centered on the player's position (legacy MapRenderer).
    private void DrawPlayer(SpriteBatch batch, TextureLibrary textures, Vector2 origin)
    {
        const int size = MapView.CellPixels;
        var position = _exploration.PlayerPosition;
        var left = (int)Math.Round((position.X * size) - (size / 2.0) - origin.X);
        var top = (int)Math.Round((position.Y * size) - (size / 2.0) - origin.Y);
        var source = new Rectangle(
            _walkAnimation.Frame * FieldSpriteFramePixels,
            WalkAnimation.RowOf(_exploration.PlayerFacing) * FieldSpriteFramePixels,
            FieldSpriteFramePixels,
            FieldSpriteFramePixels);
        batch.Draw(textures.Get(ArtFolders.FieldSprites, _fieldSpriteFile), new Rectangle(left, top, size, size), source, Color.White);
    }
}
