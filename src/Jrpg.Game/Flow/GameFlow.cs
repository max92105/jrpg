using Jrpg.Core.Content;
using Jrpg.Core.Session;
using Jrpg.Game.Audio;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Saving;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Flow;

/// <summary>
/// Owns the screens and the one playthrough (ADR 0008), and changes screens behind a fade to black when a scene asks.
/// Its update never touches the graphics device, so it can run headless in tests; drawing is separate.
/// </summary>
internal sealed class GameFlow : IScene, IFreezesFrame
{
    // The fade for a new game, a load or the title (placeholder timing).
    private static readonly TimeSpan _fadeOut = TimeSpan.FromSeconds(0.5);
    private static readonly TimeSpan _blackHold = TimeSpan.FromSeconds(0.15);
    private static readonly TimeSpan _fadeIn = TimeSpan.FromSeconds(0.5);

    private readonly SceneStack _scenes = new();
    private readonly FadeGate _fade = new(new ScreenFade(_fadeOut, _blackHold, _fadeIn));
    private readonly SessionTransitions _sessions;
    private readonly MenuTransitions _menus;
    private readonly BattleTransitions _battles;

    // A battle asked for waits through the flash over the frozen field, then goes behind the fade (decision 258).
    private readonly EncounterFlash _flash = new();
    private BattleRequested? _flashingBattle;
    private Texture2D? _frozenFrame;

    // From the flash's peak the frozen frame is drawn broken into shards; each encounter breaks it its own way (a
    // placeholder seed plus the encounters started so far).
    private const int ShatterSeed = 258;
    private static readonly Point _impact = new(VirtualScreen.Width / 2, VirtualScreen.Height / 2);
    private int _encountersStarted;
    private ShatterLayout? _shatter;

    // The shards fly apart over the fade-out, from the update the fade starts; a new flight for each encounter.
    private EncounterShatter _shatterFlight = new(_fadeOut);

    // One for the game's lifetime, made on the first draw that needs it; the graphics device frees its effects when the
    // game closes, so the flow, which tests make by the hundred without a device, needn't be disposable.
    private ShatterView? _shatterView;

    /// <param name="catalog">The content, as loaded.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="saveSlots">The player's save files.</param>
    /// <param name="settings">The player's options, which the Options page edits and dialogue reads.</param>
    public GameFlow(ContentCatalog catalog, ScreenText text, SaveSlots saveSlots, PlayerSettings settings)
    {
        var factory = new SceneFactory(catalog, text, saveSlots, settings);
        _sessions = new SessionTransitions(catalog, _scenes, factory);
        _menus = new MenuTransitions(_scenes, factory);
        _battles = new BattleTransitions(_scenes, factory);
        Voice = factory.Voice;
        _scenes.Push(factory.CreateTitle());
    }

    /// <summary>The voice-over the game wants heard, for the host to play after each update.</summary>
    public VoiceChannel Voice { get; }

    /// <summary>The playthrough, once one has started; null on the title until a game starts.</summary>
    public GameSession? Session { get; private set; }

    /// <summary>Whether an encounter's flash waits for a copy of the field's last frame to show still.</summary>
    public bool WantsFrozenFrame => _flashingBattle is not null && _frozenFrame is null;

    /// <summary>
    /// Play time counts every update while a game is on, menus and pause included (developer decision 35). The scenes
    /// get no input while the fade plays, and a request one returns meanwhile waits for the fade to end.
    /// </summary>
    /// <returns>Quit, which only the host can do; null otherwise, since the flow handles every other request.</returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        Session?.AddPlayTime(delta);
        AdvanceFlash(delta);
        _fade.Update(delta);
        ApplyRequestOnceBlack();
        if (_fade.TakeReleased() is { } released)
        {
            Start(released);
        }

        var request = _scenes.Update(delta, IsChanging ? default : input);
        if (request is null or QuitRequested)
        {
            return request;
        }

        if (IsChanging)
        {
            _fade.Hold(request);
        }
        else
        {
            Start(request);
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        _scenes.Draw(batch, textures);
        DrawFrozenFrame(batch, textures);
        var blackness = _fade.Blackness;
        if (blackness > 0)
        {
            batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), Color.Black * blackness);
        }
    }

    private void Start(SceneRequest request)
    {
        switch (request)
        {
            case BattleRequested battle:
                _flashingBattle = battle;
                _encountersStarted++;
                _shatterFlight = new EncounterShatter(_fadeOut);
                _flash.Start();
                break;
            case MenuRequested:
                _menus.Open(Session ?? throw new InvalidOperationException("The menu opened with no game started."));
                break;
            case MenuClosed:
                _menus.Close();
                break;
            default:
                _fade.StartBehindFade(request);
                break;
        }
    }

    public void FreezeFrame(Texture2D frame)
    {
        _frozenFrame = frame;
        _shatter = ShatterLayout.Break(_impact, ShatterSeed + _encountersStarted);
    }

    // The screen changes while the fade plays and while an encounter flashes: the scenes get no input meanwhile.
    private bool IsChanging => _fade.IsFading || _flashingBattle is not null;

    // Before the fade's own update, so the fade starts on the update the flash ends and one long update runs both.
    private void AdvanceFlash(TimeSpan delta)
    {
        _flash.Advance(delta);
        _shatterFlight.Advance(delta);
        if (_flashingBattle is { } battle && !_flash.IsPlaying)
        {
            _flashingBattle = null;
            _fade.StartBehindFade(battle);
            _shatterFlight.Start();
        }
    }

    // The field, still, from the encounter until the battle starts at black, under the flash and the fade; once broken,
    // its shards fly apart over black, so the field flies away into the dark. The white waits for the copy: with two
    // updates before the first draw the flash is already lit, and the copy would keep it.
    private void DrawFrozenFrame(SpriteBatch batch, TextureLibrary textures)
    {
        if (_frozenFrame is null)
        {
            return;
        }

        var screen = new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height);
        if (_flash.IsPastPeak && _shatter is not null)
        {
            batch.Draw(textures.Pixel, screen, Color.Black);
            PrepareShatterView(batch.GraphicsDevice, _shatter).Draw(batch, _frozenFrame, _shatterFlight);
        }
        else
        {
            batch.Draw(_frozenFrame, screen, Color.White);
        }

        batch.Draw(textures.Pixel, screen, Color.White * _flash.Whiteness);
    }

    // Made on the first draw that needs it, since only drawing has the device; its vertices are built once per layout.
    private ShatterView PrepareShatterView(GraphicsDevice device, ShatterLayout layout)
    {
        _shatterView ??= new ShatterView(device);
        if (!_shatterView.IsBuiltFor(layout))
        {
            _shatterView.BuildVertices(layout);
        }

        return _shatterView;
    }

    private void ApplyRequestOnceBlack()
    {
        switch (_fade.TakeDueAtBlack())
        {
            case null:
                break;
            case NewGameRequested:
                _sessions.ShowLeaderChoice();
                break;
            case LeaderChosen chosen:
                Session = _sessions.StartNewGame(chosen.Choice);
                break;
            case LoadRequested load:
                Session = _sessions.Restore(load.Save);
                break;
            case TitleRequested:
                _sessions.ShowTitle();
                Session = null;
                break;
            case BattleRequested:
                _frozenFrame = null;
                _shatter = null;
                _battles.Start(Session ?? throw new InvalidOperationException("A battle started with no game started."));
                break;
            case BattleFinished:
                _battles.Finish();
                break;
            case var request:
                throw new InvalidOperationException($"The game can't handle a {request.GetType().Name} yet.");
        }
    }
}
