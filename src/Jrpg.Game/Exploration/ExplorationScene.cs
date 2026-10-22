using System.Diagnostics.CodeAnalysis;
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
    // Legacy GameManager: the camera catches up quickly while the player walks and settles gently once they stop.
    private const double WalkingCameraShare = 0.35;
    private const double StandingCameraShare = 0.1;

    private readonly ExplorationState _exploration;
    // Built once: a method group converted on every call would allocate a delegate per frame.
    private readonly Func<PlacedObject, ObjectDefinition> _currentDefinitionOf;
    private readonly MapArt _art;
    private readonly EventPresenter _presenter;
    private readonly InteractionPromptView _prompt;
    private readonly MapCamera _camera = new();
    private readonly MapDrawList _drawList = new();
    private readonly WalkAnimation _walkAnimation = new();
    private MapDefinition _map;
    private MapView _view;

    public ExplorationScene(ExplorationState exploration, MapArt art, EventPresenter presenter, InteractionPromptView prompt)
    {
        _exploration = exploration;
        _currentDefinitionOf = exploration.CurrentDefinitionOf;
        _art = art;
        _presenter = presenter;
        _prompt = prompt;
        EnterCurrentMap();
    }

    /// <summary>
    /// Shows the running event's step; interacts, walks and opens the menu only once it and its fade are over. An
    /// interaction that starts an event waits for it like any other. While a step's encounter is pending, the player
    /// stands. The prompt over what the player faces follows, hidden while an event shows.
    /// </summary>
    /// <returns>
    /// The menu when Menu is pressed between events; a battle on the update a step starts an encounter; otherwise null.
    /// </returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        var request = UpdateExploration(delta, input);
        _prompt.Update(_presenter.IsBusy ? null : _exploration.InteractionInFront, input.Device, delta);
        return request;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        DrawMap(batch, textures);
        _prompt.Draw(batch, textures, _camera.Origin);
        _presenter.Draw(batch, textures);
    }

    private SceneRequest? UpdateExploration(TimeSpan delta, InputFrame input)
    {
        // Sampled before the presenter runs: the Confirm that closes the last message mustn't also interact again.
        var wasBusy = _presenter.IsBusy;
        _presenter.Update(delta, input);
        if (!ReferenceEquals(_exploration.CurrentMap, _map))
        {
            EnterCurrentMap();
        }

        // Core refuses walking and interacting until the battle the encounter started is finished.
        if (_exploration.PendingEncounter is not null)
        {
            _walkAnimation.Stand();
            return null;
        }

        if (!wasBusy && input.IsPressed(GameButton.Confirm))
        {
            _exploration.TryInteract();
        }

        if (wasBusy || _presenter.IsBusy)
        {
            _walkAnimation.Stand();
            return null;
        }

        if (input.IsPressed(GameButton.Menu))
        {
            _walkAnimation.Stand();
            return new MenuRequested();
        }

        WalkAndFollow(delta, input);
        return _exploration.PendingEncounter is null ? null : new BattleRequested();
    }

    // The one way onto a map, at the start and after each warp: the camera jumps rather than gliding across the map.
    [MemberNotNull(nameof(_map), nameof(_view))]
    private void EnterCurrentMap()
    {
        _map = _exploration.CurrentMap;
        _view = new MapView(_map.Layout, _currentDefinitionOf, _art.Atlas);
        _camera.SnapTo(_exploration.PlayerPosition, _map.Layout);
        _drawList.Rebuild(_map.Layout, _currentDefinitionOf, _exploration.PlayerPosition);
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
        _drawList.Rebuild(_map.Layout, _currentDefinitionOf, position);
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
                    _view.DrawCanopy(batch, _art.Canopies.CanopyOf(_currentDefinitionOf(placed).Id, textures), placed, origin);
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
        var source = FieldSpriteFrame.SourceOf(_exploration.PlayerFacing, _walkAnimation.Frame);
        batch.Draw(textures.Get(ArtFolders.FieldSprites, _art.PlayerSpriteFile), new Rectangle(left, top, size, size), source, Color.White);
    }
}
