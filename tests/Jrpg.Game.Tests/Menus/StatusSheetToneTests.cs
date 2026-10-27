using Jrpg.Game.Menus;

namespace Jrpg.Game.Tests.Menus;

public sealed class StatusSheetToneTests
{
    [Theory]
    // The tone as an int: xUnit's public test method can't take the internal enum.
    [InlineData(-1, (int)ResistanceTone.Weak)]
    [InlineData(0, (int)ResistanceTone.Plain)]
    [InlineData(749, (int)ResistanceTone.Plain)]
    [InlineData(750, (int)ResistanceTone.Capped)]
    public void ToneOf_Permille_IsWeakBelowZeroAndCappedAtTheCap(int permille, int expected)
    {
        Assert.Equal((ResistanceTone)expected, StatusSheet.ToneOf(permille));
    }
}
