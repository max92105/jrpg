using Microsoft.Xna.Framework;

namespace Jrpg.Game;

internal sealed class JrpgGame : Microsoft.Xna.Framework.Game
{
    private static readonly TimeSpan _simulationStep = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);

    public JrpgGame()
    {
        // The constructor registers the manager in Services; Game finds it there, uses it, and disposes it.
        _ = new GraphicsDeviceManager(this);
        IsFixedTimeStep = true;
        TargetElapsedTime = _simulationStep;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        base.Draw(gameTime);
    }
}
