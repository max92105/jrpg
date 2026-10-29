using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Crafting;

/// <summary>
/// The Forge screen a forge master's event opens over the map (decision 353). Cancel asks to close it, once: a second
/// request would make the flow pop what lies under it.
/// </summary>
internal sealed class ForgeScene(ScreenText text) : IScene
{
    private readonly ForgeView _view = new(text);
    private bool _isClosing;

    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        if (_isClosing || !input.IsPressed(GameButton.Cancel))
        {
            return null;
        }

        _isClosing = true;
        return new ForgeClosed();
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures) => _view.Draw(batch, textures);
}
