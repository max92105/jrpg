using System.Diagnostics;
using Jrpg.Core.Content;
using Jrpg.Core.Randomness;
using Jrpg.Core.Saving;
using Jrpg.Core.Session;
using Jrpg.Game.Scenes;

namespace Jrpg.Game.Flow;

/// <summary>Starting and leaving playthroughs (ADR 0008). A new game, a loaded one, and back to the title.</summary>
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

    /// <summary>
    /// Continues from a save on a fresh seed, logged like a new game's, since the random state isn't saved (developer
    /// decision 43). Its map replaces every scene on show.
    /// </summary>
    /// <param name="save">Fitted to the current content.</param>
    public GameSession Restore(SaveGame save)
    {
        var seed = Environment.TickCount;
        Debug.WriteLine($"Loaded game seed: {seed}");
        var session = GameSession.Restore(save, catalog, new SeededRandom(seed));
        scenes.ReplaceAll(factory.CreateExploration(session.Exploration));
        return session;
    }

    /// <summary>Ends the playthrough on screen: the title replaces the map and anything over it.</summary>
    public void ShowTitle() => scenes.ReplaceAll(factory.CreateTitle());
}
