using FontStashSharp;
using Jrpg.Core.Saving;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Saving;

/// <summary>
/// The save files, under a heading that says what choosing one does (Save, Load). Read from disk each time it opens, so
/// it shows saves made since.
/// </summary>
internal sealed class SaveFilesPanel
{
    // Placeholder layout (decision 82): a 28 x 20 margin, a heading title, then the list below it.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int HeadingPixels = 30;
    private const int ListTop = 60;

    private readonly SaveSlots _slots;
    private readonly ScreenText _text;
    private readonly UiTextures _ui;
    private readonly SpriteFontBase _headingFont;
    private readonly string _heading;
    private SaveSlotList? _list;
    private SaveSlotListView? _view;

    /// <param name="slots">The files.</param>
    /// <param name="headingId">The message the panel is titled with.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared gradient textures.</param>
    public SaveFilesPanel(SaveSlots slots, string headingId, ScreenText text, UiTextures ui)
    {
        _slots = slots;
        _text = text;
        _ui = ui;
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _heading = text.Messages.Text(headingId);
    }

    /// <summary>The files as last read; empty until the panel is opened.</summary>
    public IReadOnlyList<SaveSlotEntry> Entries => _list?.Entries ?? [];

    public void Open()
    {
        _list = new SaveSlotList(_slots.ReadAll());
        _view = new SaveSlotListView(_list, _text, _ui);
    }

    /// <summary>Reads the files again, after one changed; the cursor stays.</summary>
    /// <exception cref="InvalidOperationException">The panel wasn't opened.</exception>
    public void Refresh() => OpenList().Refresh(_slots.ReadAll());

    /// <returns>The file confirmed this update, or null.</returns>
    /// <exception cref="InvalidOperationException">The panel wasn't opened.</exception>
    public SaveSlotEntry? Update(InputFrame input) => OpenList().Update(input);

    /// <summary>
    /// Moves through the files and loads the readable one confirmed. A file that turned unreadable since the list was
    /// read is shown so instead (the reason is logged), and the list stays.
    /// </summary>
    /// <returns>The save to continue from, fitted to the current content; null on every other update.</returns>
    /// <exception cref="InvalidOperationException">The panel wasn't opened.</exception>
    public SaveGame? ChooseToLoad(InputFrame input)
    {
        if (Update(input) is not { IsReadable: true } chosen)
        {
            return null;
        }

        var save = _slots.Load(chosen.Number);
        if (save is null)
        {
            Refresh();
        }

        return save;
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel and gradients come from.</param>
    /// <param name="area">The menu's page area.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    /// <exception cref="InvalidOperationException">The panel wasn't opened.</exception>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var view = _view ?? throw NotOpened();
        var left = area.X + Margin;
        var top = area.Y + TopMargin;
        batch.DrawString(_headingFont, _heading, new Vector2(left, top), UiTheme.TextPrimary * opacity);
        view.Draw(batch, textures, new Rectangle(left, top + ListTop, area.Right - Margin - left, 0), opacity);
    }

    private static InvalidOperationException NotOpened() => new("Open the save files before using them.");

    private SaveSlotList OpenList() => _list ?? throw NotOpened();
}
