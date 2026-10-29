using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// The row look the Status and Equipment pages and the Forge share (S1b, E2, F1c): a small spaced-caps label at the
/// row's left and a large value ending where the row says, both centered on the row, so every sheet reads one way.
/// </summary>
internal sealed class SheetRowLook
{
    /// <summary>The small spaced-caps labels' size; the Status page's other labels share it.</summary>
    public const int LabelPixels = 18;

    /// <summary>The large values' size; the Status page's other values share it.</summary>
    public const int ValuePixels = 26;

    /// <summary>The labels' letter spacing.</summary>
    public const float LabelSpacing = 2;

    private readonly SpriteFontBase _labelFont;
    private readonly SpriteFontBase _valueFont;

    public SheetRowLook(ScreenText text)
    {
        _labelFont = text.Fonts.FontAt(LabelPixels);
        _valueFont = text.Fonts.FontAt(ValuePixels);
    }

    public MeasuredText Measure(string value) => MeasuredText.Of(_valueFont, value);

    /// <summary>The label at the row's left and the value ending at its right edge.</summary>
    public void Draw(SpriteBatch batch, Rectangle row, Cell cell, float opacity)
    {
        DrawLabel(batch, row, cell.Label, opacity);
        DrawValue(batch, row, new Value(cell.Value, cell.ValueColor, row.Right), opacity);
    }

    public void DrawLabel(SpriteBatch batch, Rectangle row, string label, float opacity)
    {
        var position = new Vector2(row.X, Middle(row) - (_labelFont.LineHeight / 2f));
        batch.DrawString(_labelFont, label, position, UiTheme.TextSecondary * opacity, characterSpacing: LabelSpacing);
    }

    public void DrawValue(SpriteBatch batch, Rectangle row, Value value, float opacity)
    {
        var position = new Vector2(value.Right - value.Text.Width, Middle(row) - (_valueFont.LineHeight / 2f));
        batch.DrawString(_valueFont, value.Text.Text, position, value.Color * opacity);
    }

    private static float Middle(Rectangle row) => row.Y + (row.Height / 2f);

    /// <summary>One row's label, its value, and the value's color.</summary>
    public readonly record struct Cell(string Label, MeasuredText Value, Color ValueColor);

    /// <summary>A value as drawn: its measured text, its color, and the x it ends at.</summary>
    public readonly record struct Value(MeasuredText Text, Color Color, float Right);
}
