using Jrpg.Core.Session;
using Jrpg.Game.Scenes;

namespace Jrpg.Game.Flow;

/// <summary>Opening the game menu over the map and closing it again (ADR 0008); no fade, the menu fades itself in.</summary>
internal sealed class MenuTransitions(SceneStack scenes, SceneFactory factory)
{
    /// <param name="session">The playthrough the menu shows.</param>
    public void Open(GameSession session) => scenes.Push(factory.CreateGameMenu(session));

    public void Close() => scenes.Pop();
}
