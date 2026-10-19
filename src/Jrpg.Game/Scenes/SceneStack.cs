using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
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

    /// <summary>Swaps everything on show for one scene, such as the title for the map and the menu over it.</summary>
    public void ReplaceAll(IScene scene)
    {
        _scenes.Clear();
        _scenes.Add(scene);
    }

    /// <returns>The top scene's request; null when it has none or the stack is empty.</returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input) => _scenes.Count > 0 ? _scenes[^1].Update(delta, input) : null;

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        for (var index = 0; index < _scenes.Count; index++)
        {
            _scenes[index].Draw(batch, textures);
        }
    }
}
