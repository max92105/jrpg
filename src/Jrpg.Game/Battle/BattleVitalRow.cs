using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// One vital in a battle panel: "HP", the value large, "/max" small, all right-aligned, over a thin bar. Texts are
/// measured only when their string changes.
/// </summary>
internal sealed class BattleVitalRow
{
    // Placeholder margins: 10 px between the label and the value, 2 px before the maximum.
    private const int LabelGap = 10;
    private const int MaxGap = 2;
    private const int BarHeight = 3;

    private readonly VitalRowStyle _style;
    private readonly MeasuredText _label;
    private readonly StatBar _bar;
    private MeasuredText _value = new(string.Empty, 0);
    private MeasuredText _max = new(string.Empty, 0);

    /// <param name="style">Fonts, width and offsets shared by every panel's row of this vital.</param>
    /// <param name="label">"HP", in the style's label font.</param>
    /// <param name="bar">The thin bar under the value, in the vital's colors.</param>
    public BattleVitalRow(VitalRowStyle style, MeasuredText label, StatBar bar)
    {
        _style = style;
        _label = label;
        _bar = bar;
    }

    /// <summary>The row's height: the value's line and the bar under it.</summary>
    public int Height => (int)_style.Value.Font.LineHeight + BarHeight;

    /// <param name="valueText">The value as text, from <see cref="VitalsText"/>.</param>
    /// <param name="maxText">"/max", from <see cref="VitalsText"/>.</param>
    /// <param name="value">The value, for the bar.</param>
    /// <param name="maximum">The maximum, for the bar.</param>
    public void Show(string valueText, string maxText, int value, int maximum)
    {
        _bar.Show(value, maximum);
        if (!ReferenceEquals(valueText, _value.Text))
        {
            _value = MeasuredText.Of(_style.Value.Font, valueText);
        }

        if (!ReferenceEquals(maxText, _max.Text))
        {
            _max = MeasuredText.Of(_style.Max.Font, maxText);
        }
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel and the bar's gradient come from.</param>
    /// <param name="topRight">The row's top right corner.</param>
    /// <param name="valueColor">The value's color, already faded: the rest of the row fades as much.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Point topRight, Color valueColor)
    {
        var opacity = valueColor.A / 255f;
        var valueLine = _style.Value.Font.LineHeight;
        var smallTop = topRight.Y + valueLine - _style.SmallTextLift;
        var maxLeft = topRight.X - _max.Width;
        _style.Max.Draw(batch, _max.Text, new Vector2(maxLeft, smallTop - _style.Max.Font.LineHeight), UiTheme.TextPrimary * opacity);
        var valueLeft = maxLeft - MaxGap - _value.Width;
        _style.Value.Draw(batch, _value.Text, new Vector2(valueLeft, topRight.Y), valueColor);
        var labelLeft = valueLeft - LabelGap - _label.Width;
        _style.Label.Draw(batch, _label.Text, new Vector2(labelLeft, smallTop - _style.Label.Font.LineHeight), UiTheme.TextSecondary * opacity);
        var bar = new Rectangle(topRight.X - _style.Width, topRight.Y + (int)valueLine, _style.Width, BarHeight);
        _bar.Draw(batch, textures, bar, opacity);
    }
}
