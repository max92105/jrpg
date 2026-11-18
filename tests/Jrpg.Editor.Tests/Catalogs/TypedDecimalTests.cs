using System.Globalization;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// A decimal box as typed: a dot, a sign and an exponent read, a comma or a number the content can't hold refused,
// whatever the machine's culture.
public sealed class TypedDecimalTests
{
    // A culture writing decimals with a comma, so a read or a write following the machine's culture fails here.
    private static readonly CultureInfo _commaCulture = CultureInfo.GetCultureInfo("fr-CA");

    [Fact]
    public void Parse_NegativeDecimal_ReadsIt()
    {
        Assert.Equal(-1.5, TypedDecimal.Parse(" -1.5 "));
    }

    [Fact]
    public void Parse_UnderACommaCulture_ReadsTheDot()
    {
        var number = UnderCommaCulture(() => TypedDecimal.Parse("2.25"));

        Assert.Equal(2.25, number);
    }

    // double.TryParse reads all three whatever the styles (.claude/rules/editor.md).
    [Theory]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-Infinity")]
    public void Parse_NotFinite_IsNull(string text)
    {
        Assert.Null(TypedDecimal.Parse(text));
    }

    [Fact]
    public void Parse_CommaForDecimals_IsNull()
    {
        Assert.Null(TypedDecimal.Parse("1,5"));
    }

    // Text writes the tiny and the huge one with an exponent ("1E-05", "1E+300"), which Parse must read back.
    [Theory]
    [InlineData(2.25)]
    [InlineData(0.00001)]
    [InlineData(1e300)]
    public void Parse_Text_ReadsTheNumberBack(double number)
    {
        Assert.Equal(number, TypedDecimal.Parse(TypedDecimal.Text(number)));
    }

    [Fact]
    public void Text_UnderACommaCulture_WritesADot()
    {
        var text = UnderCommaCulture(() => TypedDecimal.Text(2.25));

        Assert.Equal("2.25", text);
    }

    [Fact]
    public void ProblemOf_Number_IsNull()
    {
        Assert.Null(TypedDecimal.ProblemOf("Start x", "3"));
    }

    [Fact]
    public void ProblemOf_Text_NamesTheLabel()
    {
        var problem = TypedDecimal.ProblemOf("Start x", " far ");

        Assert.Equal("Start x must be a number, with a dot for decimals (was 'far').", problem);
    }

    private static T UnderCommaCulture<T>(Func<T> read)
    {
        var before = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = _commaCulture;
        try
        {
            return read();
        }
        finally
        {
            CultureInfo.CurrentCulture = before;
        }
    }
}
