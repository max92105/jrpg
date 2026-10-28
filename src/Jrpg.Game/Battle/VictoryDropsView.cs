using FontStashSharp;
using Jrpg.Core.Loot;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws the victory screen's Items step where the members' cards were (decision 355): "Items" in the title serif, then
/// each drop on a pointed tab like a card's, in the cards' two columns: stacks first with their "×n", then each piece.
/// </summary>
internal sealed class VictoryDropsView
{
    // Placeholder layout on the 1920 x 1080 virtual screen (decision 56): the heading where the cards start, then 54 px
    // rows down to the footer's hairline.
    private const int HeadingTop = 404;
    private const int RowsTop = 494;
    private const int RowPitch = PointedBarShape.CardHeaderHeight + 12;
    private const int RowsPerColumn = (VictoryScreenView.FooterLineTop - RowsTop) / RowPitch;
    private const int IconLeft = 16;
    private const int IconSize = 32;
    private const int NameLeft = IconLeft + IconSize + 14;
    private const int QuantityRight = 24;
    private const int HeadingPixels = 56;
    private const int NamePixels = 26;

    /// <summary>How many rows fit; past it, the last row counts the rest.</summary>
    internal const int Capacity = 2 * RowsPerColumn;

    private readonly UiTextures _ui;
    private readonly OutlinedFont _headingFont;
    private readonly OutlinedFont _nameFont;
    private readonly string _heading;
    private readonly IReadOnlyList<VictoryDropRow> _rows;

    /// <param name="drops">What dropped.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">Where the pointed tabs and the icons are baked.</param>
    public VictoryDropsView(Drops drops, ScreenText text, UiTextures ui)
    {
        _ui = ui;
        _headingFont = new OutlinedFont(text.Fonts.TitleFontAt(HeadingPixels));
        _nameFont = new OutlinedFont(text.Fonts.FontAt(NamePixels));
        _heading = text.Messages.Text(MessageIds.BattleVictoryItems);
        _rows = RowsOf(drops, text.Messages, _nameFont.Font);
    }

    /// <summary>
    /// The rows in order, stacks then pieces; more than <see cref="Capacity"/> end on a "+n more" row.
    /// </summary>
    internal static IReadOnlyList<VictoryDropRow> RowsOf(Drops drops, UiMessages messages, SpriteFontBase font)
    {
        var rows = drops.Stacks
            .Select(stack => new VictoryDropRow(stack.Item.Name, stack.Item.IconFile, MeasuredText.Of(font, CountOf(stack, messages))))
            .Concat(drops.Gear.Select(piece => new VictoryDropRow(piece.Base.Name, piece.Base.IconFile, Quantity: null)))
            .ToList();
        if (rows.Count <= Capacity)
        {
            return rows;
        }

        var more = messages.Format(MessageIds.BattleVictoryMoreDrops, MessageArgument.Number("count", rows.Count - Capacity + 1));
        return [.. rows.Take(Capacity - 1), new VictoryDropRow(more, IconFile: null, Quantity: null)];
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the tabs and icons are made.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        var left = VictoryScreenView.CardTopLeft(0).X;
        _headingFont.Draw(batch, _heading, new Vector2(left, HeadingTop), UiTheme.TextPrimary);
        for (var index = 0; index < _rows.Count; index++)
        {
            // The first two cards stand in the two columns, so their left edges are the columns'.
            var columnLeft = VictoryScreenView.CardTopLeft(index / RowsPerColumn).X;
            var tab = new Rectangle(columnLeft, RowsTop + (index % RowsPerColumn * RowPitch), PointedBarShape.CardHeaderWidth, PointedBarShape.CardHeaderHeight);
            DrawRow(batch, textures, _rows[index], tab);
        }
    }

    private static string CountOf(ItemDrop stack, UiMessages messages) =>
        messages.Format(MessageIds.BattleVictoryDropCount, MessageArgument.Number("count", stack.Quantity));

    private void DrawRow(SpriteBatch batch, TextureLibrary textures, VictoryDropRow row, Rectangle tab)
    {
        batch.Draw(_ui.CardHeader(textures), tab, Color.White);
        if (row.IconFile is { } iconFile)
        {
            batch.Draw(_ui.Icon(textures, iconFile, IconSize), new Vector2(tab.X + IconLeft, tab.Center.Y - (IconSize / 2)), Color.White);
        }

        var textTop = tab.Center.Y - (_nameFont.Font.LineHeight / 2f);
        _nameFont.Draw(batch, row.Name, new Vector2(tab.X + NameLeft, textTop), UiTheme.TextPrimary);
        if (row.Quantity is { } quantity)
        {
            _nameFont.Draw(batch, quantity.Text, new Vector2(tab.Right - QuantityRight - quantity.Width, textTop), UiTheme.TextPrimary);
        }
    }
}
