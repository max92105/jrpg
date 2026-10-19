using System.Diagnostics;
using Jrpg.Core.Content;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Scenes;

namespace Jrpg.Game.Flow;

/// <summary>Starting and leaving playthroughs (ADR 0008). A new game and back to the title so far; loading comes in S2a.</summary>
internal sealed class SessionTransitions(ContentCatalog catalog, SceneStack scenes, SceneFactory factory)
{
    /// <summary>
    /// Starts a new game on its own seed, taken from the clock and written to the debug output so a run can be replayed
    /// (ADR 0008). Its map replaces every scene on show.
    /// </summary>
    public GameSession StartNewGame()
    {
        var seed = Environment.TickCount;
        Debug.WriteLine($"New game seed: {seed}");
        var session = GameSession.StartNew(catalog, new SeededRandom(seed));
        scenes.ReplaceAll(factory.CreateExploration(session.Exploration));
        return session;
    }

    /// <summary>Ends the playthrough on screen: the title replaces the map and anything over it.</summary>
    public void ShowTitle() => scenes.ReplaceAll(factory.CreateTitle());
}
