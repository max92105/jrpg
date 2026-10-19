using System.Diagnostics;
using Jrpg.Core.Content;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Exploration;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;

namespace Jrpg.Game.Flow;

/// <summary>Starting and leaving playthroughs (ADR 0008). Part 3a only starts a new game; load, title and quit come in 3b.</summary>
internal sealed class SessionTransitions(ContentCatalog catalog, SceneStack scenes, TileAtlas atlas, CanopyTextures canopies)
{
    /// <summary>
    /// Starts a new game on its own seed, taken from the clock and written to the debug output so a run can be replayed
    /// (ADR 0008).
    /// </summary>
    public GameSession StartNewGame()
    {
        var seed = Environment.TickCount;
        Debug.WriteLine($"New game seed: {seed}");
        var session = GameSession.StartNew(catalog, new SeededRandom(seed));
        scenes.Push(new ExplorationScene(session.Exploration, catalog, atlas, canopies));
        return session;
    }
}
