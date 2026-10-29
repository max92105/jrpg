using FontStashSharp;
using Jrpg.Game.Menus;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Crafting;

/// <summary>
/// Draws the Forge screen in the menu's frame: the map dimmed behind it, the header bar with the title, the list column
/// at the left and the detail column at the right, the footer with its hint, and the salvage prompt over it all.
/// Everything fades with the Forge (decision 370).
/// </summary>
internal sealed class ForgeView
{
    /// <summary>The rows' and lines' text size, the menu pages' body size.</summary>
    public const int BodyPixels = 26;

    // The menu header's title size and side margin, so the Forge's title reads as a screen's; the footer's hint 40 px
    // in, as the menu's.
    private const int TitleTextPixels = 38;
    private const int SideMargin = 48;
    private const int FooterTextInset = 40;

    // The columns at 1920 (placeholder): the list from x 40 to 1000, the detail from 1040 to 1880, both the menu's body
    // height.
    private const int ListLeft = 40;
    private const int ListWidth = 960;
    private const int DetailLeft = 1040;
    private const int DetailWidth = 840;

    private static readonly Rectangle _screen = new(0, 0, VirtualScreen.Width, VirtualScreen.Height);
    private static readonly Rectangle _listArea = new(ListLeft, MenuLayout.BodyTop, ListWidth, MenuLayout.BodyHeight);
    private static readonly Rectangle _detailArea = new(DetailLeft, MenuLayout.BodyTop, DetailWidth, MenuLayout.BodyHeight);

    private readonly ForgeListView _list;
    private readonly ForgeDetailView _detail;
    private readonly ConfirmPromptView _prompt;
    private readonly SpriteFontBase _titleFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly string _title;
    private readonly string _hint;

    /// <param name="text">The title, the hint, the lines and the rows' fonts.</param>
    /// <param name="ui">The shared textures: icons, the selected row, the hairlines.</param>
    /// <param name="look">Draws the material rows, whose counts it measured.</param>
    public ForgeView(ScreenText text, UiTextures ui, SheetRowLook look)
    {
        _titleFont = text.Fonts.FontAt(TitleTextPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _list = new ForgeListView(text, ui, _bodyFont);
        _detail = new ForgeDetailView(text, ui, look, _bodyFont);
        _prompt = new ConfirmPromptView(text, ui);
        _title = text.Messages.Text(MessageIds.ForgeTitle);
        _hint = text.Messages.Text(MessageIds.ForgeHint);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel and the gradients come from.</param>
    /// <param name="place">What the Forge shows this frame.</param>
    /// <param name="opacity">The Forge's fade, 0 for clear to 1 for opaque.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        batch.Draw(textures.Pixel, _screen, UiTheme.Scrim * opacity);
        HeaderBar.Draw(batch, textures.Pixel, MenuHeader.Height, opacity);
        FooterBar.Draw(batch, textures.Pixel, MenuLayout.FooterHeight, opacity);
        batch.DrawString(_titleFont, _title, new Vector2(SideMargin, (MenuHeader.Height - _titleFont.LineHeight) / 2f), UiTheme.TextPrimary * opacity);
        batch.DrawString(_bodyFont, _hint, new Vector2(FooterTextInset, FooterBar.TextTop(MenuLayout.FooterHeight, _bodyFont.LineHeight)), UiTheme.TextSecondary * opacity);

        _list.Draw(batch, textures, place.List, _listArea, opacity);
        _detail.Draw(batch, textures, new ForgeDetailView.Place(place.List.SelectedRow, place.Materials.Rows, place.Actions, place.Notice, _detailArea), opacity);
        if (place.Prompt is { } prompt)
        {
            _prompt.Draw(batch, textures, prompt, opacity);
        }
    }

    /// <summary>What the Forge shows this frame.</summary>
    /// <param name="List">The owned pieces and the cursor on them.</param>
    /// <param name="Materials">Every material and how many are owned.</param>
    /// <param name="Actions">The actions open on the highlighted piece; null while the list has the cursor.</param>
    /// <param name="Notice">What the last action gave; null when nothing was done since the cursor moved.</param>
    /// <param name="Prompt">The question before a salvage; null while nothing is asked.</param>
    public readonly record struct Place(ForgeList List, MaterialStock Materials, ForgeActionChoice? Actions, string? Notice, ConfirmPrompt? Prompt);
}
