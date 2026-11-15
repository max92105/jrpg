namespace Jrpg.Editor.Tests;

public sealed class StaThreadTests
{
    [Fact]
    public void Run_BodyThrows_RethrowsIt()
    {
        var thrown = Assert.Throws<InvalidOperationException>(
            () => StaThread.Run(() => throw new InvalidOperationException("inside")));

        Assert.Equal("inside", thrown.Message);
    }

    [Fact]
    public void Run_Body_RunsOnAnStaThread()
    {
        var apartment = ApartmentState.Unknown;

        StaThread.Run(() => apartment = Thread.CurrentThread.GetApartmentState());

        Assert.Equal(ApartmentState.STA, apartment);
    }
}
