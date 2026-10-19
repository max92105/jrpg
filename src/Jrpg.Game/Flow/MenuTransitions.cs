using Jrpg.Game.Scenes;

namespace Jrpg.Game.Flow;

/// <summary>Opening the game menu over the map and closing it again (ADR 0008); no fade, the menu fades itself in.</summary>
internal sealed class MenuTransitions(SceneStack scenes, SceneFactory factory)
{
    public void Open() => scenes.Push(factory.CreateGameMenu());

    public void Close() => scenes.Pop();
}
