using System.Collections.Immutable;
using FontStashSharp;

namespace Jrpg.Game.Ui;

/// <summary>
/// One vital's bar and its value text, in the one HP format, "45 / 60" (decision 110). The text is formatted only
/// when the values change, so showing the same values again allocates nothing.
/// </summary>
internal sealed class VitalMeter
{
    private readonly UiMessages _messages;
    private readonly SpriteFontBase _valueFont;
    private (int Value, int Max)? _shown;

    /// <param name="text">The format message and the value's font.</param>
    /// <param name="ui">Where the bar's gradient is made, once.</param>
    /// <param name="fill">The bar's gradient, such as <see cref="UiTheme.HpBar"/>.</param>
    /// <param name="valuePixels">The value text's font size, which its measured width depends on.</param>
    public VitalMeter(ScreenText text, UiTextures ui, ImmutableArray<GradientStop> fill, int valuePixels)
    {
        _messages = text.Messages;
        _valueFont = text.Fonts.FontAt(valuePixels);
        Bar = new StatBar(ui, fill);
    }

    public StatBar Bar { get; }

    /// <summary>"value / max", measured in the value font; empty until the first <see cref="Show"/>.</summary>
    public MeasuredText Value { get; private set; } = new(string.Empty, 0);

    public void Show(int value, int max)
    {
        Bar.Show(value, max);
        if (_shown == (value, max))
        {
            return;
        }

        _shown = (value, max);
        var formatted = _messages.Format(
            MessageIds.StatValueOfMax,
            MessageArgument.Number("value", value),
            MessageArgument.Number("max", max));
        Value = MeasuredText.Of(_valueFont, formatted);
    }
}
