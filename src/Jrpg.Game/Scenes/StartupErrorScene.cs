using FontStashSharp.RichText;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Scenes;

/// <summary>Shown instead of the game when content can't load, with the reason, which names the broken file.</summary>
internal sealed class StartupErrorScene : IScene
{
    private const int TextPixels = 40;
    private const int Margin = 96;

    private readonly RichTextLayout _layout;

    public StartupErrorScene(string message, FontLibrary fonts)
    {
        _layout = new RichTextLayout
        {
            // Off, so a path or a slash in the message is never read as a FontStashSharp formatting command.
            SupportsCommands = false,
            Font = fonts.FontAt(TextPixels),
            Width = VirtualScreen.Width - (2 * Margin),
            Text = message,
        };
    }

    public void Update(TimeSpan delta, InputFrame input)
    {
    }

    public void Draw(SpriteBatch batch) => _layout.Draw(batch, new Vector2(Margin, Margin), Color.White);
}
