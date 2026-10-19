using System.Diagnostics.CodeAnalysis;
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
/// objects in depth order, canopies and roofs over the player, with the camera easing after the player, and leaves map
/// events to <see cref="EventPresenter"/>.
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
    private readonly MapArt _art;
    private readonly string _fieldSpriteFile;
    private readonly EventPresenter _presenter;
    private readonly MapCamera _camera = new();
    private readonly MapDrawList _drawList = new();
    private readonly WalkAnimation _walkAnimation = new();
    private MapDefinition _map;
    private MapView _view;

    public ExplorationScene(ExplorationState exploration, ContentCatalog catalog, MapArt art)
    {
        _exploration = exploration;
        _objects = catalog.Objects;
        _art = art;
        _fieldSpriteFile = catalog.NewGame.FieldSpriteFile;
        _presenter = new EventPresenter(exploration);
        EnterCurrentMap();
    }

    /// <summary>Shows the running event's step; walks only once it and its fade are over.</summary>
    public void Update(TimeSpan delta, InputFrame input)
    {
        _presenter.Update(delta);
        if (!ReferenceEquals(_exploration.CurrentMap, _map))
        {
            EnterCurrentMap();
        }

        if (_presenter.IsBusy)
        {
            _walkAnimation.Stand();
            return;
        }

        WalkAndFollow(delta, input);
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        DrawMap(batch, textures);
        var blackness = _presenter.Blackness;
        if (blackness > 0)
        {
            batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), Color.Black * blackness);
        }
    }

    // The one way onto a map, at the start and after each warp: the camera jumps rather than gliding across the map.
    [MemberNotNull(nameof(_map), nameof(_view))]
    private void EnterCurrentMap()
    {
        _map = _exploration.CurrentMap;
        _view = new MapView(_map.Layout, _objects, _art.Atlas);
        _camera.SnapTo(_exploration.PlayerPosition, _map.Layout);
        _drawList.Rebuild(_map.Layout, _objects, _exploration.PlayerPosition);
    }

    private void WalkAndFollow(TimeSpan delta, InputFrame input)
    {
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
        _camera.Follow(position, _map.Layout, delta, isMoving ? WalkingCameraShare : StandingCameraShare);
        _drawList.Rebuild(_map.Layout, _objects, position);
    }

    private void DrawMap(SpriteBatch batch, TextureLibrary textures)
    {
        var origin = _camera.Origin;
        var drawables = _drawList.Drawables;
        for (var index = 0; index < drawables.Count; index++)
        {
            var drawable = drawables[index];
            switch (drawable.Kind)
            {
                case DrawableKind.Layer:
                    _view.DrawLayer(batch, textures, _map.Layout.Layers[drawable.Index], origin);
                    break;
                case DrawableKind.Object:
                    _view.DrawObject(batch, textures, _map.Layout.Objects[drawable.Index], origin);
                    break;
                case DrawableKind.Canopy:
                    var placed = _map.Layout.Objects[drawable.Index];
                    _view.DrawCanopy(batch, _art.Canopies.CanopyOf(placed.ObjectId, textures), placed, origin);
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
