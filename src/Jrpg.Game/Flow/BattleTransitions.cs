using Jrpg.Core.Session;
using Jrpg.Game.Scenes;

namespace Jrpg.Game.Flow;

/// <summary>Into a battle over the map and back (ADR 0013); the flow calls both once its fade is black.</summary>
internal sealed class BattleTransitions(SceneStack scenes, SceneFactory factory)
{
    /// <param name="session">The playthrough whose pending encounter is fought.</param>
    public void Start(GameSession session)
    {
        session.StartPendingBattle();
        scenes.Push(factory.CreateBattle(session));
    }

    public void Finish() => scenes.Pop();
}
