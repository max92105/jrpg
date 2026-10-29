using Jrpg.Core.Session;
using Jrpg.Game.Scenes;

namespace Jrpg.Game.Flow;

/// <summary>
/// Opening the Forge over the map and closing it again (ADR 0008); no fade, the Forge fades itself in (decision 370).
/// Closing goes on with the event that opened it.
/// </summary>
internal sealed class ForgeTransitions(SceneStack scenes, SceneFactory factory)
{
    public void Open() => scenes.Push(factory.CreateForge());

    /// <param name="session">The playthrough whose event waits on the Forge step.</param>
    public void Close(GameSession session)
    {
        scenes.Pop();
        session.Exploration.ContinueEvent();
    }
}
