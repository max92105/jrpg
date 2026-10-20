using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class VitalMeterTests
{
    private const int ValuePixels = 15;

    private readonly VitalMeter _meter = new(RepositoryScreenText.Text, new UiTextures(), UiTheme.HpBar, ValuePixels);

    [Fact]
    public void Show_HalfFull_FillsTheBarHalfway()
    {
        _meter.Show(30, 60);

        Assert.Equal(0.5f, _meter.Bar.Fraction);
    }

    [Fact]
    public void Show_Values_FormatsValueOverMax()
    {
        _meter.Show(30, 60);

        Assert.Equal("30 / 60", _meter.Value.Text);
    }

    [Fact]
    public void Show_SameValuesAgain_KeepsTheSameString()
    {
        _meter.Show(30, 60);
        var first = _meter.Value.Text;

        _meter.Show(30, 60);

        Assert.Same(first, _meter.Value.Text);
    }

    [Fact]
    public void Show_ValueChanged_Reformats()
    {
        _meter.Show(30, 60);

        _meter.Show(45, 60);

        Assert.Equal("45 / 60", _meter.Value.Text);
    }
}
