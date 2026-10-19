using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Microsoft.Xna.Framework;
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
    private readonly string _artRoot;
    private readonly PlayerSettings _settings;
    private readonly GraphicsDeviceManager _graphics;
    private readonly InputReader _input = new();
    private KeyboardState _previousKeyboard;
    private Point _windowSize = new(StartingWindowWidth, StartingWindowHeight);
    private SpriteBatch? _batch;
    private RenderTarget2D? _virtualScreen;
    private TextureLibrary? _textures;

    /// <param name="rootScene">The game flow, or the screen explaining why the game can't start.</param>
    /// <param name="artRoot">Where <see cref="TextureLibrary"/> reads images from.</param>
    /// <param name="settings">The player's options; the window follows <see cref="PlayerSettings.IsFullScreen"/>.</param>
    public JrpgGame(IScene rootScene, string artRoot, PlayerSettings settings)
    {
        _rootScene = rootScene;
        _artRoot = artRoot;
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
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var isShortcutPressed = IsActive && FullScreenShortcut.IsPressed(_previousKeyboard, keyboard);
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
        _rootScene.Update(gameTime.ElapsedGameTime, IsActive && !isShortcutPressed ? input : default);
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
            _textures?.Dispose();
        }

        base.Dispose(disposing);
    }

    private static Point ReadDisplaySize()
    {
        var mode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        return new Point(mode.Width, mode.Height);
    }

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
