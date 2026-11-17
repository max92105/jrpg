using System.Globalization;
using System.Windows.Media;
using Jrpg.Editor.Party;

namespace Jrpg.Editor.Tests.Party;

// The Resources tab's color swatch: the typed #RRGGBB's color, transparent for anything Data's color rule refuses.
public sealed class HexColorBrushConverterTests
{
    private readonly HexColorBrushConverter _converter = new();

    [Fact]
    public void Convert_SixHexDigits_GivesThatColor() => Assert.Equal(Color.FromRgb(0x5B, 0x8C, 0xFF), ColorOf("#5B8CFF"));

    [Fact]
    public void Convert_LowercaseDigits_GivesThatColor() => Assert.Equal(Color.FromRgb(0xC8, 0x32, 0xAB), ColorOf("#c832ab"));

    // The resource writes its color trimmed, so the swatch shows what is saved.
    [Fact]
    public void Convert_SurroundingSpaces_GivesThatColor() => Assert.Equal(Color.FromRgb(0x5B, 0x8C, 0xFF), ColorOf(" #5B8CFF "));

    [Fact]
    public void Convert_HalfTyped_GivesTransparent() => Assert.Equal(Colors.Transparent, ColorOf("#5B8"));

    [Fact]
    public void Convert_SevenDigits_GivesTransparent() => Assert.Equal(Colors.Transparent, ColorOf("#5B8CFF0"));

    [Fact]
    public void Convert_NoHash_GivesTransparent() => Assert.Equal(Colors.Transparent, ColorOf("5B8CFFA"));

    [Fact]
    public void Convert_NotHex_GivesTransparent() => Assert.Equal(Colors.Transparent, ColorOf("#GGGGGG"));

    // AllowHexSpecifier refuses a sign, as Data's rule does.
    [Fact]
    public void Convert_SignedDigits_GivesTransparent() => Assert.Equal(Colors.Transparent, ColorOf("#-5B8CF"));

    [Fact]
    public void ConvertBack_Throws() =>
        Assert.Throws<NotSupportedException>(
            () => _converter.ConvertBack(Brushes.Red, typeof(string), null, CultureInfo.InvariantCulture));

    private Color ColorOf(string text) =>
        Assert.IsType<SolidColorBrush>(_converter.Convert(text, typeof(Brush), null, CultureInfo.InvariantCulture)).Color;
}
