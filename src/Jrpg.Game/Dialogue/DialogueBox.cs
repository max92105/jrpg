using FontStashSharp.RichText;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Dialogue;

/// <summary>The message box at the bottom of the map: one page of text in the legacy <c>DialogueBox</c> panel.</summary>
internal sealed class DialogueBox
{
    // Legacy MainWindow placed a 600 x 120 box 20 px above the bottom; doubled for the virtual screen.
    private const int Width = 1200;
    private const int Height = 240;
    private const int BottomMargin = 40;

    // Legacy 2 px border plus 8 px padding, doubled.
    private const int TextInset = 20;

    private static readonly Rectangle _area = new((VirtualScreen.Width - Width) / 2, VirtualScreen.Height - BottomMargin - Height, Width, Height);

    private readonly PanelView _panel = new(UiTheme.MessagePanel);
    private readonly RichTextLayout _page;

    public DialogueBox(FontLibrary fonts)
    {
        _page = new RichTextLayout
        {
            // Off, so a bracket or a slash in content text is never read as a FontStashSharp formatting command.
            SupportsCommands = false,
            Font = fonts.FontAt(UiTheme.MessageTextPixels),
            Width = Width - (2 * TextInset),
        };
    }

    /// <summary>Lays the page out once, so drawing it every frame allocates nothing.</summary>
    public void Show(string page) => _page.Text = page;

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        _panel.Draw(batch, textures, _area);
        _page.Draw(batch, new Vector2(_area.X + TextInset, _area.Y + TextInset), UiTheme.MessageText);
    }
}
