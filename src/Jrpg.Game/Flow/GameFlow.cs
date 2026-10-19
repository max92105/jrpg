using Jrpg.Core.Content;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
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
    // Legacy SceneManager.TransitionAsync, for a new game, a load or the title.
    private static readonly TimeSpan _fadeOut = TimeSpan.FromSeconds(0.5);
    private static readonly TimeSpan _blackHold = TimeSpan.FromSeconds(0.15);
    private static readonly TimeSpan _fadeIn = TimeSpan.FromSeconds(0.5);

    private readonly SceneStack _scenes = new();
    private readonly ScreenFade _fade = new(_fadeOut, _blackHold, _fadeIn);
    private readonly SessionTransitions _sessions;
    private SceneRequest? _requestBehindFade;

    public GameFlow(ContentCatalog catalog, ScreenText text)
    {
        var playedCatalog = EncounterFreeContent.Of(catalog);
        _sessions = new SessionTransitions(playedCatalog, _scenes, new SceneFactory(playedCatalog, text));
        StartBehindFade(new NewGameRequested());
    }

    /// <summary>The playthrough, once one has started; null until the first fade reaches black.</summary>
    public GameSession? Session { get; private set; }

    /// <summary>
    /// Play time counts every update while a game is on, menus and pause included (developer decision 35). The scenes
    /// get no input while the fade plays.
    /// </summary>
    /// <returns>Null: the flow handles every request itself.</returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        Session?.AddPlayTime(delta);
        _fade.Update(delta);
        ApplyRequestOnceBlack();

        var isFading = _fade.Phase != FadePhase.Idle;
        var request = _scenes.Update(delta, isFading ? default : input);
        if (request is not null)
        {
            StartBehindFade(request);
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        _scenes.Draw(batch, textures);
        var blackness = _fade.Blackness;
        if (blackness > 0)
        {
            batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), Color.Black * blackness);
        }
    }

    private void StartBehindFade(SceneRequest request)
    {
        _fade.Start();
        _requestBehindFade = request;
    }

    // Past fading out rather than exactly holding: one long update can carry the fade over the whole hold.
    private void ApplyRequestOnceBlack()
    {
        if (_requestBehindFade is null || _fade.Phase == FadePhase.FadingOut)
        {
            return;
        }

        var request = _requestBehindFade;
        _requestBehindFade = null;
        Session = request switch
        {
            NewGameRequested => _sessions.StartNewGame(),
            _ => throw new InvalidOperationException($"The game can't handle a {request.GetType().Name} yet."),
        };
    }
}
