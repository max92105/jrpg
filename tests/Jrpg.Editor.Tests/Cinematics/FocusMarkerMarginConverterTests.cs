using System.Globalization;
using System.Windows;
using Jrpg.Core.Cinematics;
using Jrpg.Editor.Cinematics;

namespace Jrpg.Editor.Tests.Cinematics;

// The 16 px focus marker centered on its point of the 480 x 270 preview frame (decision 755).
public sealed class FocusMarkerMarginConverterTests
{
    private readonly FocusMarkerMarginConverter _converter = new();

    [Fact]
    public void Convert_FocusAtTheCenter_CentersTheMarker()
    {
        Assert.Equal(new Thickness(232, 127, 0, 0), MarginOf(Focus(50, 50)));
    }

    [Fact]
    public void Convert_FocusAtTheTopLeft_CentersTheMarkerOnTheCorner()
    {
        Assert.Equal(new Thickness(-8, -8, 0, 0), MarginOf(Focus(0, 0)));
    }

    [Fact]
    public void Convert_FocusAtTheBottomRight_CentersTheMarkerOnTheCorner()
    {
        Assert.Equal(new Thickness(472, 262, 0, 0), MarginOf(Focus(100, 100)));
    }

    [Fact]
    public void Convert_NoFocus_IsNoMargin()
    {
        Assert.Equal(new Thickness(), MarginOf(null));
    }

    private static StillFocus Focus(int xPercent, int yPercent) => new() { XPercent = xPercent, YPercent = yPercent };

    private Thickness MarginOf(StillFocus? focus) =>
        (Thickness)_converter.Convert(focus, typeof(Thickness), null, CultureInfo.InvariantCulture);
}
