using System.Diagnostics.CodeAnalysis;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// The player walking the current map; the scene a new game starts in. It shows the tile layers, the animated player
/// and objects in depth order, canopies and roofs over the player, with the camera easing after the player, the prompt
/// and the entry banner over the field (<see cref="FieldOverlays"/>), and leaves map events to
/// <see cref="EventPresenter"/>.
/// </summary>
internal sealed class ExplorationScene : IScene
{
    // The camera catches up quickly while walking and settles gently once stopped (placeholder shares, decision 86).
    private const double WalkingCameraShare = 0.35;
    private const double StandingCameraShare = 0.1;

    private readonly ExplorationState _exploration;
    // Built once: a method group converted on every call would allocate a delegate per frame.
    private readonly Func<PlacedObject, ObjectDefinition> _currentDefinitionOf;
    private readonly MapArt _art;
    private readonly EventPresenter _presenter;
    private readonly AskedSounds _sounds = new();
    private readonly FieldOverlays _overlays;
    private readonly MapCamera _camera = new();
    private readonly MapDrawList _drawList = new();
    private readonly WalkAnimation _walkAnimation = new();
    private MapDefinition _map;
    private MapView _view;

    public ExplorationScene(ExplorationState exploration, MapArt art, EventPresenter presenter, FieldOverlays overlays)
    {
        _exploration = exploration;
        _currentDefinitionOf = exploration.CurrentDefinitionOf;
        _art = art;
        _presenter = presenter;
        _overlays = overlays;
        EnterCurrentMap();
    }

    public IReadOnlyList<string> Sounds => _sounds.Files;

    /// <summary>The open map's animation time, for the tests (decision 464).</summary>
    internal TimeSpan TileAnimationTime => _view.AnimationTime;

    /// <summary>
    /// Shows the running event's step; interacts, walks and opens the menu only once it and its fade are over. An
    /// interaction that starts an event waits for it like any other. While a step's encounter is pending, the player
    /// stands. The prompt over what the player faces follows, hidden while an event shows; the entry banner's clock
    /// runs only between events (decision 529).
    /// </summary>
    /// <returns>
    /// The menu when Menu is pressed between events; a battle on the update a step starts an encounter; the Forge or
    /// the named shop on the update its step first shows (decision 353); otherwise null.
    /// </returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        _sounds.Clear();
        // First, so the animated tiles move on whatever the update then does: an event, an encounter's pause, a warp.
        _view.Advance(delta);
        var request = UpdateExploration(delta, input);
        var isBusy = _presenter.IsBusy;
        _overlays.Prompt.Update(isBusy ? null : _exploration.InteractionInFront, input.Device, delta);
        if (!isBusy)
        {
            _overlays.Banner.Advance(delta);
        }

        _overlays.BannerView.Update();
        return request;
    }

    // The banner under the presenter: an event's message box covers it.
    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        DrawMap(batch, textures);
        _overlays.Prompt.Draw(batch, textures, _camera.Origin);
        _overlays.BannerView.Draw(batch, textures);
        _presenter.Draw(batch, textures);
    }

    // The step says which screen: a shop step names its shop, any other opening step is the Forge's.
    private static SceneRequest ScreenRequestFor(EventStep opening) =>
        opening is OpenShopStep shop ? new ShopRequested(shop.ShopId) : new ForgeRequested();

    private SceneRequest? UpdateExploration(TimeSpan delta, InputFrame input)
    {
        // Sampled before the presenter runs: the Confirm that closes the last message mustn't also interact again.
        var wasBusy = _presenter.IsBusy;
        _presenter.Update(delta, input);
        _sounds.AskAll(_presenter.Sounds);
        if (!ReferenceEquals(_exploration.CurrentMap, _map))
        {
            EnterCurrentMap();
        }

        if (_presenter.OpeningStep is { } opening)
        {
            _walkAnimation.Stand();
            return ScreenRequestFor(opening);
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
            _sounds.Ask(UiSoundFiles.Effects.MenuConfirm);
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
        _overlays.Banner.Enter(_map);
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
        var items = _drawList.Items;
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            switch (item.Kind)
            {
                case MapDrawItemKind.Layer:
                    _view.DrawLayer(batch, textures, _map.Layout.Layers[item.Index], origin);
                    break;
                case MapDrawItemKind.ObjectBody:
                    _view.DrawObject(batch, textures, _map.Layout.Objects[item.Index], origin);
                    break;
                case MapDrawItemKind.Canopy:
                    var placed = _map.Layout.Objects[item.Index];
                    _view.DrawCanopy(batch, _art.Canopies.CanopyOf(_currentDefinitionOf(placed).Id, textures), placed, origin);
                    break;
                case MapDrawItemKind.Player:
                    DrawPlayer(batch, textures, origin);
                    break;
            }
        }
    }

    // The sprite is one cell square, centered on the player's position.
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
