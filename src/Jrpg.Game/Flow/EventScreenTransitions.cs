using Jrpg.Core.Session;
using Jrpg.Game.Scenes;

namespace Jrpg.Game.Flow;

/// <summary>
/// Opening a screen a map event asks for (the Forge, a shop) over the map and closing it again (ADR 0008); no fade, the
/// screen fades itself in (decision 370). Closing goes on with the event that opened it.
/// </summary>
internal sealed class EventScreenTransitions(SceneStack scenes, SceneFactory factory)
{
    /// <param name="session">The playthrough whose owned pieces the Forge lists.</param>
    public void OpenForge(GameSession session) => scenes.Push(factory.CreateForge(session));

    /// <param name="session">The playthrough whose gold and shop stock the Shop shows.</param>
    /// <param name="shopId">The shop the event named.</param>
    public void OpenShop(GameSession session, string shopId) => scenes.Push(factory.CreateShop(session, shopId));

    /// <param name="session">The playthrough whose event waits on the screen's step.</param>
    public void Close(GameSession session)
    {
        scenes.Pop();
        session.Exploration.ContinueEvent();
    }
}
