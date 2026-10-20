using FontStashSharp;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Saving;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Saving from the menu (legacy <c>SaveLoadView</c> in save mode): the 20 files, and Confirm writes the playthrough into
/// the selected one. Overwriting doesn't ask yet (S3).
/// </summary>
internal sealed class SavePage
{
    // Legacy SaveLoadView: a 28 x 20 margin, the Text.Heading title, then the list below it.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int HeadingPixels = 22;
    private const int ListTop = 48;

    private readonly SaveSlots _slots;
    private readonly GameSession _session;
    private readonly ScreenText _text;
    private readonly UiTextures _ui;
    private readonly SpriteFontBase _headingFont;
    private readonly string _heading;
    private SaveSlotList? _list;
    private SaveSlotListView? _view;

    /// <param name="slots">The files.</param>
    /// <param name="session">What a save writes.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared gradient textures.</param>
    public SavePage(SaveSlots slots, GameSession session, ScreenText text, UiTextures ui)
    {
        _slots = slots;
        _session = session;
        _text = text;
        _ui = ui;
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _heading = text.Messages.Text(MessageIds.SystemSave);
    }

    /// <summary>The files as last read; empty until the page is opened.</summary>
    public IReadOnlyList<SaveSlotEntry> Entries => _list?.Entries ?? [];

    /// <summary>Reads the files, each time the page opens, so it shows saves made since.</summary>
    public void Open()
    {
        _list = new SaveSlotList(_slots.ReadAll());
        _view = new SaveSlotListView(_list, _text, _ui);
    }

    /// <exception cref="InvalidOperationException">The page wasn't opened.</exception>
    public void Update(InputFrame input)
    {
        var list = _list ?? throw new InvalidOperationException("Open the Save page before updating it.");
        if (list.Update(input) is { } chosen)
        {
            _slots.Write(chosen.Number, _session);
            list.Refresh(_slots.ReadAll());
        }
    }

    /// <summary>Draws in the System page's place, in the menu's page area.</summary>
    /// <exception cref="InvalidOperationException">The page wasn't opened.</exception>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var view = _view ?? throw new InvalidOperationException("Open the Save page before drawing it.");
        var left = area.X + Margin;
        var top = area.Y + TopMargin;
        batch.DrawString(_headingFont, _heading, new Vector2(left, top), UiTheme.TextPrimary * opacity);
        view.Draw(batch, textures, new Rectangle(left, top + ListTop, area.Right - Margin - left, 0), opacity);
    }
}
