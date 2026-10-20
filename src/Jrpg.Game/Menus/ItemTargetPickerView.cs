using System.Globalization;
using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws an <see cref="ItemTargetPicker"/> over the dimmed menu, in legacy <c>DialogStyle</c>'s raised band: "Use item",
/// then one row per member with HP and MP, the ones the item can't be used on dimmed and the chosen ones lit. The legacy
/// portrait cards come in P2b-2.
/// </summary>
internal sealed class ItemTargetPickerView
{
    // Legacy DialogStyle padding 32; Text.Heading 22 px with 24 px below; rows of Text.Body 18 px.
    private const int BandPadding = 32;
    private const int HeadingPixels = 22;
    private const int BodyPixels = 18;
    private const int HeadingGap = 24;
    private const int RowHeight = 40;
    private const int RowWidth = 640;
    private const int RowInset = 16;

    private readonly UiMessages _messages;
    private readonly UiTextures _ui;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private string _heading = string.Empty;
    private float _headingWidth;
    private MemberRow[] _rows = [];

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared gradient textures.</param>
    public ItemTargetPickerView(ScreenText text, UiTextures ui)
    {
        _messages = text.Messages;
        _ui = ui;
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
    }

    /// <summary>Builds the text once, when the picker opens.</summary>
    /// <param name="picker">The members and who can be chosen.</param>
    /// <param name="itemName">The item being used.</param>
    public void Show(ItemTargetPicker picker, string itemName)
    {
        _heading = _messages.Format(MessageIds.ItemUseTitle, new MessageArgument("item", itemName));
        _headingWidth = _headingFont.MeasureString(_heading).X;
        _rows = [.. picker.Members.Select(member =>
        {
            var vitals = _messages.Format(
                MessageIds.ItemTargetVitals,
                Argument("hp", member.Vitals.Hp),
                Argument("maxHp", member.Vitals.MaxHp),
                Argument("mp", member.Vitals.Mp),
                Argument("maxMp", member.Vitals.MaxMp));
            return new MemberRow(member.Definition.Name, vitals, _bodyFont.MeasureString(vitals).X);
        })];
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel and gradients come from.</param>
    /// <param name="picker">The same picker <see cref="Show"/> was given.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, ItemTargetPicker picker, float opacity)
    {
        var bandHeight = (2 * BandPadding) + (int)_headingFont.LineHeight + HeadingGap + (_rows.Length * RowHeight);
        var band = new Rectangle(0, (VirtualScreen.Height - bandHeight) / 2, VirtualScreen.Width, bandHeight);
        batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim * opacity);
        batch.Draw(textures.Pixel, band, UiTheme.RaisedSurface * opacity);
        batch.Draw(textures.Pixel, new Rectangle(0, band.Top, VirtualScreen.Width, 1), UiTheme.BarLine * opacity);
        batch.Draw(textures.Pixel, new Rectangle(0, band.Bottom - 1, VirtualScreen.Width, 1), UiTheme.BarLine * opacity);
        batch.DrawString(_headingFont, _heading, new Vector2((VirtualScreen.Width - _headingWidth) / 2, band.Top + BandPadding), UiTheme.TextPrimary * opacity);

        var rowsTop = band.Top + BandPadding + (int)_headingFont.LineHeight + HeadingGap;
        for (var index = 0; index < _rows.Length; index++)
        {
            var row = new Rectangle((VirtualScreen.Width - RowWidth) / 2, rowsTop + (index * RowHeight), RowWidth, RowHeight);
            var member = picker.Members[index];
            DrawRow(batch, textures, row, _rows[index], new RowState(picker.IsCandidate(member), picker.IsSelected(member), opacity));
        }
    }

    private static MessageArgument Argument(string name, int value) => new(name, value.ToString(CultureInfo.InvariantCulture));

    private void DrawRow(SpriteBatch batch, TextureLibrary textures, Rectangle row, MemberRow text, RowState state)
    {
        if (state.IsSelected)
        {
            batch.Draw(_ui.Gradient(textures, UiTheme.SelectedRow), row, Color.White * state.Opacity);
            var line = _ui.Gradient(textures, UiTheme.HighlightLine);
            batch.Draw(line, new Rectangle(row.X, row.Y, row.Width, 1), Color.White * state.Opacity);
            batch.Draw(line, new Rectangle(row.X, row.Bottom - 1, row.Width, 1), Color.White * state.Opacity);
        }

        var color = (state.IsCandidate ? UiTheme.TextPrimary : UiTheme.TextDisabled) * state.Opacity;
        var textTop = row.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
        batch.DrawString(_bodyFont, text.Name, new Vector2(row.X + RowInset, textTop), color);
        batch.DrawString(_bodyFont, text.Vitals, new Vector2(row.Right - RowInset - text.VitalsWidth, textTop), color);
    }

    // One member's text, built when the picker opens.
    private sealed record MemberRow(string Name, string Vitals, float VitalsWidth);

    // How a row is drawn this frame.
    private readonly record struct RowState(bool IsCandidate, bool IsSelected, float Opacity);
}
