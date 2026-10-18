using Jrpg.Game.Flow;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game;

/// <summary>The MonoGame host: steps the game flow at a fixed rate and shows the virtual screen fitted to the window.</summary>
internal sealed class JrpgGame : Microsoft.Xna.Framework.Game
{
    private static readonly TimeSpan _simulationStep = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);

    // The window opens at two thirds of the virtual screen, so it fits a 1080p desktop with room for the taskbar.
    private const int StartingWindowWidth = VirtualScreen.Width * 2 / 3;
    private const int StartingWindowHeight = VirtualScreen.Height * 2 / 3;

    private readonly GameFlow _flow;
    private SpriteBatch? _batch;
    private RenderTarget2D? _virtualScreen;

    public JrpgGame(GameFlow flow)
    {
        _flow = flow;

        // The constructor registers the manager in Services; Game finds it there, uses it, and disposes it.
        _ = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = StartingWindowWidth,
            PreferredBackBufferHeight = StartingWindowHeight,
        };
        IsFixedTimeStep = true;
        TargetElapsedTime = _simulationStep;
        Window.AllowUserResizing = true;
    }

    protected override void LoadContent()
    {
        _batch = new SpriteBatch(GraphicsDevice);
        _virtualScreen = new RenderTarget2D(GraphicsDevice, VirtualScreen.Width, VirtualScreen.Height);
    }

    protected override void Update(GameTime gameTime)
    {
        _flow.Update(gameTime.ElapsedGameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var batch = _batch!;
        GraphicsDevice.SetRenderTarget(_virtualScreen);
        GraphicsDevice.Clear(Color.Black);
        batch.Begin(samplerState: SamplerState.PointClamp);
        _flow.Draw(batch);
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
        }

        base.Dispose(disposing);
    }
}
