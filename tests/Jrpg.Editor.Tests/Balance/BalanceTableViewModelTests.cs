using Jrpg.Core.Content;
using Jrpg.Core.Progression;
using Jrpg.Editor.Balance;

namespace Jrpg.Editor.Tests.Balance;

// The balance table's boxes (decision 992): three typed levels and a typed damage modifier, read into the class skill
// rows, which empty while a box can't be read.
public sealed class BalanceTableViewModelTests
{
    private const int MaxLevel = 30;

    private const string FirstLevel = nameof(BalanceTableViewModel.FirstLevelText);
    private const string SecondLevel = nameof(BalanceTableViewModel.SecondLevelText);
    private const string ThirdLevel = nameof(BalanceTableViewModel.ThirdLevelText);
    private const string ModifierPercent = nameof(BalanceTableViewModel.ModifierPercentText);

    // The fixtures' classes over an experience table ending at level 30, so the boxes' top bound is known.
    private static readonly ContentCatalog _world = BalanceFixtures.World with
    {
        Rules = BalanceFixtures.World.Rules with { Experience = new ExperienceTable([.. Enumerable.Repeat(100, MaxLevel - 1)]) },
    };

    [Fact]
    public void New_Boxes_StartAtOneTenTwentyAndZero()
    {
        var table = new BalanceTableViewModel(_world);

        Assert.Equal(
            ("1", "10", "20", "0"),
            (table.FirstLevelText, table.SecondLevelText, table.ThirdLevelText, table.ModifierPercentText));
        Assert.Null(table.BoxesProblem);
    }

    // The mage's Power is 15, 18 and 99 at levels 2, 3 and 30; fire_1 gives 2 per Power.
    [Fact]
    public void ClassSkillRows_LevelsTyped_ReadAtThoseLevels()
    {
        var table = new BalanceTableViewModel(_world);

        Type(table, FirstLevel, "2");
        Type(table, SecondLevel, "3");
        Type(table, ThirdLevel, "30");

        Assert.Equal(["30", "36", "198"], FireballOf(table).AmountTexts);
    }

    // At levels 1, 10 and 20 Fireball reads 24, 78 and 138; 50 % more is 500 permille on the modifiers' sum.
    [Fact]
    public void ClassSkillRows_ModifierTyped_PassedAsPermille()
    {
        var table = new BalanceTableViewModel(_world);

        Type(table, ModifierPercent, "50");

        Assert.Equal(["36", "117", "207"], FireballOf(table).AmountTexts);
    }

    [Fact]
    public void BoxesProblem_LevelPastTheMax_SaysSoAndEmptiesTheRows()
    {
        var table = new BalanceTableViewModel(_world);

        Type(table, SecondLevel, "31");

        Assert.Equal("Level 2 must be a whole number from 1 to 30.", table.BoxesProblem);
        Assert.Empty(table.ClassSkillRows);
    }

    [Theory]
    [InlineData(FirstLevel, "1")]
    [InlineData(FirstLevel, "30")]
    [InlineData(SecondLevel, "1")]
    [InlineData(SecondLevel, "30")]
    [InlineData(ThirdLevel, "1")]
    [InlineData(ThirdLevel, "30")]
    [InlineData(ModifierPercent, "-99")]
    [InlineData(ModifierPercent, "10000")]
    public void BoxesProblem_BoxAtABound_NoneAndRowsListed(string box, string text)
    {
        var table = new BalanceTableViewModel(_world);

        Type(table, box, text);

        Assert.Null(table.BoxesProblem);
        Assert.NotEmpty(table.ClassSkillRows);
    }

    [Theory]
    [InlineData(FirstLevel, "0", "Level 1")]
    [InlineData(FirstLevel, "31", "Level 1")]
    [InlineData(FirstLevel, "ten", "Level 1")]
    [InlineData(SecondLevel, "0", "Level 2")]
    [InlineData(SecondLevel, "ten", "Level 2")]
    [InlineData(ThirdLevel, "0", "Level 3")]
    [InlineData(ThirdLevel, "31", "Level 3")]
    [InlineData(ThirdLevel, "", "Level 3")]
    public void BoxesProblem_LevelRefused_NamesTheBoxAndEmptiesTheRows(string box, string text, string label)
    {
        var table = new BalanceTableViewModel(_world);

        Type(table, box, text);

        Assert.Equal($"{label} must be a whole number from 1 to 30.", table.BoxesProblem);
        Assert.Empty(table.ClassSkillRows);
    }

    [Theory]
    [InlineData("-100")]
    [InlineData("10001")]
    [InlineData("2147483647")]
    [InlineData("half")]
    public void BoxesProblem_ModifierRefused_SaysSoAndEmptiesTheRows(string text)
    {
        var table = new BalanceTableViewModel(_world);

        Type(table, ModifierPercent, text);

        Assert.Equal("Damage modifier must be a whole number from -99 to 10000.", table.BoxesProblem);
        Assert.Empty(table.ClassSkillRows);
    }

    [Theory]
    [InlineData(FirstLevel)]
    [InlineData(SecondLevel)]
    [InlineData(ThirdLevel)]
    [InlineData(ModifierPercent)]
    public void TypedBox_Changed_RaisesRowsAndProblem(string box)
    {
        var table = new BalanceTableViewModel(_world);
        var raised = new List<string?>();
        table.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        Type(table, box, "5");

        Assert.Contains(nameof(BalanceTableViewModel.ClassSkillRows), raised);
        Assert.Contains(nameof(BalanceTableViewModel.BoxesProblem), raised);
    }

    private static SkillBalanceRow FireballOf(BalanceTableViewModel table) =>
        Assert.Single(table.ClassSkillRows, row => row.SkillName == BalanceFixtures.Fireball.Name);

    private static void Type(BalanceTableViewModel table, string box, string text)
    {
        switch (box)
        {
            case FirstLevel:
                table.FirstLevelText = text;
                break;
            case SecondLevel:
                table.SecondLevelText = text;
                break;
            case ThirdLevel:
                table.ThirdLevelText = text;
                break;
            case ModifierPercent:
                table.ModifierPercentText = text;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(box), box, "Not one of the table's boxes.");
        }
    }
}
