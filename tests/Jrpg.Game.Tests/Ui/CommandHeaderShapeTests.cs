using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

public sealed class CommandHeaderShapeTests
{
    private const int Width = CommandHeaderShape.Width;
    private const int Height = CommandHeaderShape.Height;

    private readonly Color[] _pixels =
        CommandHeaderShape.Pixels(Enumerable.Repeat(Color.Blue, Width).ToArray(), Color.White);

    [Fact]
    public void Pixels_Tips_AreClear()
    {
        Assert.Equal(Color.Transparent, _pixels[0]);
        Assert.Equal(Color.Transparent, _pixels[Width - 1]);
        Assert.Equal(Color.Transparent, _pixels[((Height - 1) * Width) + 0]);
        Assert.Equal(Color.Transparent, _pixels[((Height - 1) * Width) + Width - 1]);
    }

    [Fact]
    public void Pixels_Middle_IsFilled()
    {
        Assert.Equal(Color.Blue, _pixels[((Height / 2) * Width) + (Width / 2)]);
    }
}
