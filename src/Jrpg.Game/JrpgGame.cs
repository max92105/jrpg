using Jrpg.Data.Art;
using Jrpg.Data.Audio;
using Jrpg.Game.Audio;
using Jrpg.Game.Flow;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Jrpg.Game;

/// <summary>
/// The MonoGame host: steps the game at a fixed rate, shows the virtual screen fitted to the window, and switches between
/// full screen and a window when the player's settings change.
/// </summary>
internal sealed class JrpgGame : Microsoft.Xna.Framework.Game
{
    private static readonly TimeSpan _simulationStep = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);

    // The window opens at two thirds of the virtual screen, so it fits a 1080p desktop with room for the taskbar.
    private const int StartingWindowWidth = VirtualScreen.Width * 2 / 3;
    private const int StartingWindowHeight = VirtualScreen.Height * 2 / 3;

    private readonly IScene _rootScene;

    // The root scene when it is the game flow, which may ask for the field's frozen frame; null on the error screen.
    private readonly GameFlow? _flow;
    private readonly string _artRoot;
    private readonly string _audioRoot;
    private readonly VoiceChannel _voice;
    private readonly PlayerSettings _settings;
    private readonly GraphicsDeviceManager _graphics;
    private readonly InputReader _input = new();
    private KeyboardState _previousKeyboard;
    private Point _windowSize = new(StartingWindowWidth, StartingWindowHeight);
    private SpriteBatch? _batch;
    private RenderTarget2D? _virtualScreen;

    // The field's last frame before a battle (decision 258), copied from the virtual screen; created on first use.
    private RenderTarget2D? _frozenFrame;
    private TextureLibrary? _textures;

#if DEBUG
    // F12 is a developer tool, not a game action, so it is read here rather than through the input bindings.
    private readonly CaptureFiles _captures = new(GamePaths.CaptureDirectory);
    private bool _isCaptureAsked;
#endif

    // Null when the machine has no sound device: the game then runs silent (ADR 0018).
    private SoundLibrary? _sounds;
    private VoicePlayer? _voicePlayer;

    /// <param name="rootScene">The game flow, or the screen explaining why the game can't start.</param>
    /// <param name="contentRoot">The content folder, holding the art and audio folders.</param>
    /// <param name="settings">The player's options; the window follows <see cref="PlayerSettings.IsFullScreen"/>.</param>
    /// <param name="voice">The voice-over the root scene asks for; played after each of its updates.</param>
    public JrpgGame(IScene rootScene, string contentRoot, PlayerSettings settings, VoiceChannel voice)
    {
        _rootScene = rootScene;
        _flow = rootScene as GameFlow;
        _artRoot = ArtFolders.RootIn(contentRoot);
        _audioRoot = AudioFolders.RootIn(contentRoot);
        _voice = voice;
        _settings = settings;

        // Borderless full screen (no hardware mode switch) keeps the desktop resolution, so Alt+Tab and the debugger stay quick.
        var size = BackBufferSize();
        _graphics = new GraphicsDeviceManager(this)
        {
            HardwareModeSwitch = false,
            IsFullScreen = settings.IsFullScreen,
            PreferredBackBufferWidth = size.X,
            PreferredBackBufferHeight = size.Y,
        };
        IsFixedTimeStep = true;
        TargetElapsedTime = _simulationStep;
        Window.AllowUserResizing = true;
    }

    protected override void LoadContent()
    {
        _batch = new SpriteBatch(GraphicsDevice);
        _virtualScreen = new RenderTarget2D(GraphicsDevice, VirtualScreen.Width, VirtualScreen.Height);
        _textures = new TextureLibrary(GraphicsDevice, _artRoot);

        // The menu's 4096 px background takes over 200 ms to decode: on the first opening that one long frame made the
        // fixed step catch up past the menu's fade (plan 2026-10-25-ui-second-pass, BG1). Loaded here, before play.
        _textures.Get(ArtFolders.Backgrounds, UiArtFiles.Backgrounds.Menu);
        StartAudio();
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var isShortcutPressed = IsActive && FullScreenShortcut.IsPressed(_previousKeyboard, keyboard);
#if DEBUG
        _isCaptureAsked |= IsActive && keyboard.IsKeyDown(Keys.F12) && _previousKeyboard.IsKeyUp(Keys.F12);
#endif
        _previousKeyboard = keyboard;
        if (isShortcutPressed)
        {
            _settings.IsFullScreen = !_settings.IsFullScreen;
        }

        if (_settings.IsFullScreen != _graphics.IsFullScreen)
        {
            ApplyWindowMode();
        }

        // No dead zone from MonoGame: StickReader applies the game's own. Input is still read while another window has
        // focus, so a key held across the switch isn't seen as a new press, but the game doesn't act on it (decision 83).
        // Alt+Enter's Enter is the shortcut's, not a Confirm.
        var input = _input.Read(keyboard, GamePad.GetState(PlayerIndex.One, GamePadDeadZone.None));
        if (_rootScene.Update(gameTime.ElapsedGameTime, IsActive && !isShortcutPressed ? input : default) is QuitRequested)
        {
            Exit();
        }

        _voicePlayer?.Follow(_voice);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var batch = _batch!;
        GraphicsDevice.SetRenderTarget(_virtualScreen);
        GraphicsDevice.Clear(Color.Black);
        batch.Begin(samplerState: SamplerState.PointClamp);
        _rootScene.Draw(batch, _textures!);
        batch.End();
#if DEBUG
        SaveAskedCapture();
#endif
        FreezeFrameIfWanted(batch);

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);
        batch.Begin(samplerState: SamplerState.PointClamp);
        batch.Draw(_virtualScreen, VirtualScreen.DestinationIn(GraphicsDevice.PresentationParameters.Bounds), Color.White);
        batch.End();
        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _batch?.Dispose();
            _virtualScreen?.Dispose();
            _frozenFrame?.Dispose();
            _textures?.Dispose();
            _voicePlayer?.Dispose();
            _sounds?.Dispose();
        }

        base.Dispose(disposing);
    }

    // Copies the frame just drawn, in the draw so the update never touches the device. The virtual screen is never set
    // as the target again this frame: a render target discards its contents when set.
    private void FreezeFrameIfWanted(SpriteBatch batch)
    {
        if (_flow is not { WantsFrozenFrame: true })
        {
            return;
        }

        _frozenFrame ??= new RenderTarget2D(GraphicsDevice, VirtualScreen.Width, VirtualScreen.Height);
        GraphicsDevice.SetRenderTarget(_frozenFrame);
        batch.Begin(samplerState: SamplerState.PointClamp);
        batch.Draw(_virtualScreen, Vector2.Zero, Color.White);
        batch.End();
        _flow.FreezeFrame(_frozenFrame);
    }

    private static Point ReadDisplaySize()
    {
        var mode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        return new Point(mode.Width, mode.Height);
    }

    // The audio system is touched once here, so a missing device shows at startup rather than mid-intro.
    private void StartAudio()
    {
        try
        {
            // Building a sound is what opens the device (setting the master volume doesn't); one silent sample will do.
            using var probe = new SoundEffect(new byte[2], 22050, AudioChannels.Mono);
        }
        catch (NoAudioHardwareException exception)
        {
            DeveloperLog.Report($"No sound device: {exception.Message} The game plays on silently.");
            return;
        }

        _sounds = new SoundLibrary(_audioRoot);
        _voicePlayer = new VoicePlayer(_sounds);
    }

#if DEBUG
    // The virtual screen, so a capture is 1920 x 1080 whatever the window's size.
    private void SaveAskedCapture()
    {
        if (!_isCaptureAsked)
        {
            return;
        }

        _isCaptureAsked = false;
        Directory.CreateDirectory(_captures.Folder);
        var path = _captures.NextPath(DateTime.Now);
        using (var file = File.Create(path))
        {
            _virtualScreen!.SaveAsPng(file, VirtualScreen.Width, VirtualScreen.Height);
        }

        DeveloperLog.Report($"Capture saved: {path}");
    }
#endif

    private Point BackBufferSize() => _settings.IsFullScreen ? ReadDisplaySize() : _windowSize;

    // Leaving full screen restores the window as the player last sized it.
    private void ApplyWindowMode()
    {
        if (_settings.IsFullScreen)
        {
            _windowSize = Window.ClientBounds.Size;
        }

        var size = BackBufferSize();
        _graphics.PreferredBackBufferWidth = size.X;
        _graphics.PreferredBackBufferHeight = size.Y;
        _graphics.IsFullScreen = _settings.IsFullScreen;
        _graphics.ApplyChanges();
    }
}
