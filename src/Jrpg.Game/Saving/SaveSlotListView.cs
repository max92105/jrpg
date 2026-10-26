using System.Globalization;
using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Saving;

/// <summary>
/// Draws a <see cref="SaveSlotList"/>: column titles over a hairline, then one 112 px row per file with its name, its
/// location over the save date and play time, and the active party, the selected row lit.
/// </summary>
internal sealed class SaveSlotListView
{
    // Placeholder layout (decision 82): 112 px rows, 16 px in, a 140 px file column; text at 30 px for headings, 26 px
    // for the body, 18 px for captions and labels.
    private const int RowHeight = 112;
    private const int RowInset = 16;
    private const int FileColumnWidth = 140;
    private const int HeadingPixels = 30;
    private const int BodyPixels = 26;
    private const int CaptionPixels = 18;
    private const int DetailsGap = 2;
    private const string DateFormat = "yyyy-MM-dd HH:mm";

    private readonly SaveSlotList _list;
    private readonly UiMessages _messages;
    private readonly UiTextures _ui;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly SpriteFontBase _captionFont;
    private readonly ColumnTitles _columnTitles;
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
        _columnTitles = new ColumnTitles(
            _captionFont,
            ui,
            [
                ColumnTitle.FromLeft(MeasuredText.Of(_captionFont, _messages.Text(MessageIds.SaveColumnFile)), RowInset),
                ColumnTitle.FromLeft(MeasuredText.Of(_captionFont, _messages.Text(MessageIds.SaveColumnLocation)), RowInset + FileColumnWidth),
                ColumnTitle.FromRight(MeasuredText.Of(_captionFont, _messages.Text(MessageIds.SaveColumnParty)), RowInset),
            ]);
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

        _columnTitles.Draw(batch, textures, area, opacity);
        var rowsTop = area.Y + ColumnTitles.Height;
        var band = _list.Selection.BandOver(new Rectangle(area.X, rowsTop, area.Width, RowHeight));
        _ui.DrawSelectedRow(batch, textures, band, opacity);
        MenuPointer.Draw(batch, textures.Pixel, _list.Selection.PointerAt(band), opacity);
        var last = Math.Min(_rows.Length, _list.FirstVisible + SaveSlotList.VisibleRows);
        for (var index = _list.FirstVisible; index < last; index++)
        {
            var row = new Rectangle(area.X, rowsTop + ((index - _list.FirstVisible) * RowHeight), area.Width, RowHeight);
            DrawRow(batch, textures.Pixel, row, _rows[index], opacity);
        }
    }

    // Built once per read of the files, so drawing formats nothing.
    private RowText TextOf(SaveSlotEntry entry)
    {
        var file = _messages.Format(MessageIds.SaveFile, MessageArgument.Number("number", entry.Number));
        if (entry.Summary is not { } summary)
        {
            var state = _messages.Text(entry.IsDamaged ? MessageIds.SaveDamaged : MessageIds.SaveEmpty);
            return new RowText(file, state, null, new MeasuredText(string.Empty, 0));
        }

        var details = _messages.Format(
            MessageIds.SaveDetails,
            new MessageArgument("date", summary.SavedAt.ToLocalTime().ToString(DateFormat, CultureInfo.InvariantCulture)),
            new MessageArgument("time", PlayTimeText.Format(summary.PlayTime)));
        var party = string.Join("   ", summary.ActiveParty.Select(member =>
            _messages.Format(MessageIds.SaveMember, new MessageArgument("name", member.Name), MessageArgument.Number("level", member.Level))));
        return new RowText(file, summary.LocationName, details, MeasuredText.Of(_bodyFont, party));
    }

    private void DrawRow(SpriteBatch batch, Texture2D pixel, Rectangle row, RowText text, float opacity)
    {
        batch.Draw(pixel, new Rectangle(row.X, row.Bottom - 1, row.Width, 1), UiTheme.Stroke * opacity);
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
        var partyLeft = row.Right - RowInset - text.Party.Width;
        batch.DrawString(_bodyFont, text.Party.Text, new Vector2(partyLeft, middle - (_bodyFont.LineHeight / 2f)), UiTheme.TextPrimary * opacity);
    }

    // One row's text; Details is null for an empty or unreadable file, whose Location then says which it is.
    private sealed record RowText(string File, string Location, string? Details, MeasuredText Party);
}
