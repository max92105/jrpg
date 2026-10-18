using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Scenes;

/// <summary>
/// The screens on show, such as a menu over the map: all are drawn bottom to top, only the top one is updated.
/// </summary>
internal sealed class SceneStack
{
    private readonly List<IScene> _scenes = [];

    public void Push(IScene scene) => _scenes.Add(scene);

    /// <exception cref="InvalidOperationException">Only one scene is left; the game always shows one.</exception>
    public void Pop()
    {
        if (_scenes.Count <= 1)
        {
            throw new InvalidOperationException("The last scene can't be popped; replace it instead.");
        }

        _scenes.RemoveAt(_scenes.Count - 1);
    }

    /// <exception cref="InvalidOperationException">The stack is empty.</exception>
    public void Replace(IScene scene)
    {
        if (_scenes.Count == 0)
        {
            throw new InvalidOperationException("There's no scene to replace; push one first.");
        }

        _scenes[^1] = scene;
    }

    public void Update(TimeSpan delta)
    {
        if (_scenes.Count > 0)
        {
            _scenes[^1].Update(delta);
        }
    }

    public void Draw(SpriteBatch batch)
    {
        for (var index = 0; index < _scenes.Count; index++)
        {
            _scenes[index].Draw(batch);
        }
    }
}
