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
internal sealed class GameFlow : IScene
{
    // The fade for a new game, a load or the title (placeholder timing).
    private static readonly TimeSpan _fadeOut = TimeSpan.FromSeconds(0.5);
    private static readonly TimeSpan _blackHold = TimeSpan.FromSeconds(0.15);
    private static readonly TimeSpan _fadeIn = TimeSpan.FromSeconds(0.5);

    // The shatter (decision 258) breaks from the screen's center. Its seed is an arbitrary number plus the encounters
    // so far, so each break differs.
    private const int ShatterSeed = 258;
    private static readonly Point _screenCenter = new(VirtualScreen.Width / 2, VirtualScreen.Height / 2);

    private readonly SceneStack _scenes = new();
    private readonly FadeGate _fade = new(new ScreenFade(_fadeOut, _blackHold, _fadeIn));
    private readonly SessionTransitions _sessions;
    private readonly MenuTransitions _menus;
    private readonly BattleTransitions _battles;
    private readonly EncounterFlash _flash = new();
    private readonly ShatterView _shatterView = new();

    // The battle asked for, held while the flash plays (decision 258), then handed to the fade.
    private BattleRequested? _encounter;

    // The field's last frame, from the host; shown instead of the scenes until the battle starts at black.
    private Texture2D? _frozenFrame;
    private int _encounterCount;

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

    /// <summary>
    /// Whether the flow wants the screen just drawn, to keep as the field's frozen frame: once, right after a battle is
    /// asked for. The host copies it and calls <see cref="FreezeFrame"/>; the update never touches the device.
    /// </summary>
    public bool WantsFrozenFrame => _encounter is not null && _frozenFrame is null;

    /// <summary>Whether the encounter's flash plays, before the fade to the battle.</summary>
    internal bool IsFlashing => _encounter is not null;

    /// <summary>
    /// Play time counts every update while a game is on, menus and pause included (developer decision 35). The scenes
    /// get no input while the fade plays, and a request one returns meanwhile waits for the fade to end; they don't
    /// update at all while an encounter's flash plays.
    /// </summary>
    /// <returns>Quit, which only the host can do; null otherwise, since the flow handles every other request.</returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        Session?.AddPlayTime(delta);
        // The flash first, so an update that ends it also runs the fade it starts.
        AdvanceEncounterFlash(delta);
        _fade.Update(delta);
        ApplyRequestOnceBlack();
        if (_fade.TakeReleased() is { } released)
        {
            Start(released);
        }

        // The field is frozen while the flash plays, so the scenes wait.
        if (IsFlashing)
        {
            return null;
        }

        var request = _scenes.Update(delta, _fade.IsFading ? default : input);
        if (request is null or QuitRequested)
        {
            return request;
        }

        if (_fade.IsFading)
        {
            _fade.Hold(request);
        }
        else
        {
            Start(request);
        }

        return null;
    }

    /// <summary>
    /// Keeps <paramref name="frame"/> as the frozen field until the battle starts, and breaks it into the shards it
    /// shows from the flash's peak; the host owns the frame.
    /// </summary>
    public void FreezeFrame(Texture2D frame)
    {
        _frozenFrame = frame;
        _shatterView.Show(ShatterLayout.Break(_screenCenter, ShatterSeed + _encounterCount));
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        DrawScenesOrFrozenFrame(batch, textures);
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
            case MenuRequested:
                _menus.Open(Session ?? throw new InvalidOperationException("The menu opened with no game started."));
                break;
            case MenuClosed:
                _menus.Close();
                break;
            case BattleRequested battle:
                _encounter = battle;
                _encounterCount++;
                _flash.Start();
                break;
            default:
                _fade.StartBehindFade(request);
                break;
        }
    }

    // When the flash has played, the battle goes behind the usual fade, still over the frozen frame.
    private void AdvanceEncounterFlash(TimeSpan delta)
    {
        if (_encounter is null)
        {
            return;
        }

        _flash.Advance(delta);
        if (!_flash.IsPlaying)
        {
            _fade.StartBehindFade(_encounter);
            _encounter = null;
        }
    }

    // The frame cracks into shards at the flash's peak. The white is drawn only over the frozen frame, so the frame the
    // host copies never holds any of it.
    private void DrawScenesOrFrozenFrame(SpriteBatch batch, TextureLibrary textures)
    {
        if (_frozenFrame is null)
        {
            _scenes.Draw(batch, textures);
            return;
        }

        var screen = new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height);
        if (_flash.IsPastPeak)
        {
            _shatterView.Draw(batch, _frozenFrame, textures.Pixel);
        }
        else
        {
            batch.Draw(_frozenFrame, screen, Color.White);
        }

        batch.Draw(textures.Pixel, screen, Color.White * _flash.Whiteness);
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
