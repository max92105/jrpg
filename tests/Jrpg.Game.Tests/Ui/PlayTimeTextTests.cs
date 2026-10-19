using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class PlayTimeTextTests
{
    [Fact]
    public void Update_OverAnHour_ShowsHoursMinutesSeconds()
    {
        var text = new PlayTimeText();

        text.Update(new TimeSpan(1, 2, 3) + TimeSpan.FromMilliseconds(900));

        Assert.Equal("01:02:03", text.Text);
    }

    [Fact]
    public void Update_PastNinetyNineHours_KeepsEveryHourDigit()
    {
        var text = new PlayTimeText();

        text.Update(TimeSpan.FromHours(123));

        Assert.Equal("123:00:00", text.Text);
    }

    [Fact]
    public void Update_SameSecond_KeepsSameText()
    {
        var text = new PlayTimeText();
        text.Update(TimeSpan.FromSeconds(5.1));
        var first = text.Text;

        text.Update(TimeSpan.FromSeconds(5.9));

        Assert.Same(first, text.Text);
    }

    [Fact]
    public void Update_NextSecond_ChangesText()
    {
        var text = new PlayTimeText();
        text.Update(TimeSpan.FromSeconds(5.9));

        text.Update(TimeSpan.FromSeconds(6));

        Assert.Equal("00:00:06", text.Text);
    }
}
