using System.Globalization;
using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Saving;

/// <summary>
/// Draws a <see cref="SaveSlotList"/> as legacy <c>SaveLoadView</c>: column titles over a hairline, then one 92 px row per
/// file with its name, its location over the save date and play time, and the active party, the selected row lit.
/// </summary>
internal sealed class SaveSlotListView
{
    // Legacy SaveSlotPanel: 92 px rows, 16 px in, a 140 px file column; Text.Heading 22, Text.Body 18, Text.Caption and
    // Text.Label 14.
    private const int RowHeight = 92;
    private const int RowInset = 16;
    private const int FileColumnWidth = 140;
    private const int HeadingPixels = 22;
    private const int BodyPixels = 18;
    private const int CaptionPixels = 14;
    private const int ColumnTitlesHeight = 28;
    private const int DetailsGap = 2;
    private const int ColumnTitleLineGap = 20;
    private const string DateFormat = "yyyy-MM-dd HH:mm";

    private readonly SaveSlotList _list;
    private readonly UiMessages _messages;
    private readonly UiTextures _ui;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly SpriteFontBase _captionFont;
    private readonly string[] _columnTitles;
    private readonly float _partyTitleWidth;
    private IReadOnlyList<SaveSlotEntry>? _shownEntries;
    private RowText[] _rows = [];

    /// <param name="list">The files and the cursor.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared gradient textures.</param>
    public SaveSlotListView(SaveSlotList list, ScreenText text, UiTextures ui)
    {
        _list = list;
        _messages = text.Messages;
        _ui = ui;
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _captionFont = text.Fonts.FontAt(CaptionPixels);
        _columnTitles =
        [
            _messages.Text(MessageIds.SaveColumnFile),
            _messages.Text(MessageIds.SaveColumnLocation),
            _messages.Text(MessageIds.SaveColumnParty),
        ];
        _partyTitleWidth = _captionFont.MeasureString(_columnTitles[2]).X;
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel and gradients come from.</param>
    /// <param name="area">The column titles' top-left and the rows' width; the rows go below.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        if (!ReferenceEquals(_list.Entries, _shownEntries))
        {
            _shownEntries = _list.Entries;
            _rows = [.. _shownEntries.Select(TextOf)];
        }

        DrawColumnTitles(batch, textures, area, opacity);
        var rowsTop = area.Y + ColumnTitlesHeight;
        var last = Math.Min(_rows.Length, _list.FirstVisible + SaveSlotList.VisibleRows);
        for (var index = _list.FirstVisible; index < last; index++)
        {
            var row = new Rectangle(area.X, rowsTop + ((index - _list.FirstVisible) * RowHeight), area.Width, RowHeight);
            DrawRow(batch, textures, row, index, opacity);
        }
    }

    // Built once per read of the files, so drawing formats nothing.
    private RowText TextOf(SaveSlotEntry entry)
    {
        var file = _messages.Format(MessageIds.SaveFile, new MessageArgument("number", Number(entry.Number)));
        if (entry.Summary is not { } summary)
        {
            var state = _messages.Text(entry.IsDamaged ? MessageIds.SaveDamaged : MessageIds.SaveEmpty);
            return new RowText(file, state, null, string.Empty, 0);
        }

        var details = _messages.Format(
            MessageIds.SaveDetails,
            new MessageArgument("date", summary.SavedAt.ToLocalTime().ToString(DateFormat, CultureInfo.InvariantCulture)),
            new MessageArgument("time", PlayTimeText.Format(summary.PlayTime)));
        var party = string.Join("   ", summary.ActiveParty.Select(member =>
            _messages.Format(MessageIds.SaveMember, new MessageArgument("name", member.Name), new MessageArgument("level", Number(member.Level)))));
        return new RowText(file, summary.LocationName, details, party, _bodyFont.MeasureString(party).X);
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    // Legacy SectionHeaderStyle over the same three columns as the rows.
    private void DrawColumnTitles(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var color = UiTheme.TextSecondary * opacity;
        var left = area.X + RowInset;
        batch.DrawString(_captionFont, _columnTitles[0], new Vector2(left, area.Y), color);
        batch.DrawString(_captionFont, _columnTitles[1], new Vector2(left + FileColumnWidth, area.Y), color);
        batch.DrawString(_captionFont, _columnTitles[2], new Vector2(area.Right - RowInset - _partyTitleWidth, area.Y), color);
        batch.Draw(_ui.SectionLine(textures), new Rectangle(area.X, area.Y + ColumnTitleLineGap, area.Width, 1), Color.White * opacity);
    }

    private void DrawRow(SpriteBatch batch, TextureLibrary textures, Rectangle row, int index, float opacity)
    {
        var text = _rows[index];
        if (index == _list.Selected)
        {
            batch.Draw(_ui.Gradient(textures, UiTheme.Highlight), row, Color.White * opacity);
            var line = _ui.Gradient(textures, UiTheme.HighlightLine);
            batch.Draw(line, new Rectangle(row.X, row.Y, row.Width, 1), Color.White * opacity);
            batch.Draw(line, new Rectangle(row.X, row.Bottom - 1, row.Width, 1), Color.White * opacity);
        }

        batch.Draw(textures.Pixel, new Rectangle(row.X, row.Bottom - 1, row.Width, 1), UiTheme.Stroke * opacity);
        var middle = row.Y + (RowHeight / 2f);
        var left = row.X + RowInset;
        batch.DrawString(_headingFont, text.File, new Vector2(left, middle - (_headingFont.LineHeight / 2f)), UiTheme.TextPrimary * opacity);

        var detailsLeft = left + FileColumnWidth;
        if (text.Details is null)
        {
            batch.DrawString(_bodyFont, text.Location, new Vector2(detailsLeft, middle - (_bodyFont.LineHeight / 2f)), UiTheme.TextDisabled * opacity);
            return;
        }

        var blockTop = middle - ((_bodyFont.LineHeight + DetailsGap + _captionFont.LineHeight) / 2f);
        batch.DrawString(_bodyFont, text.Location, new Vector2(detailsLeft, blockTop), UiTheme.TextPrimary * opacity);
        batch.DrawString(_captionFont, text.Details, new Vector2(detailsLeft, blockTop + _bodyFont.LineHeight + DetailsGap), UiTheme.TextSecondary * opacity);
        var partyLeft = row.Right - RowInset - text.PartyWidth;
        batch.DrawString(_bodyFont, text.Party, new Vector2(partyLeft, middle - (_bodyFont.LineHeight / 2f)), UiTheme.TextPrimary * opacity);
    }

    // One row's text; Details is null for an empty or unreadable file, whose Location then says which it is.
    private sealed record RowText(string File, string Location, string? Details, string Party, float PartyWidth);
}
