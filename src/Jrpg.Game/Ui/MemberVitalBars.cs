using Jrpg.Core.Stats;

namespace Jrpg.Game.Ui;

/// <summary>A member's HP meter, shown from their vitals.</summary>
internal sealed class MemberVitalBars
{
    /// <param name="text">The format message and the values' font.</param>
    /// <param name="ui">Where the bars' gradients are made, once.</param>
    /// <param name="valuePixels">The value texts' font size.</param>
    public MemberVitalBars(ScreenText text, UiTextures ui, int valuePixels)
    {
        Hp = new VitalMeter(text, ui, UiTheme.HpBar, valuePixels);
    }

    public VitalMeter Hp { get; }

    public void Show(Vitals vitals)
    {
        Hp.Show(vitals.Hp, vitals.MaxHp);
    }
}
