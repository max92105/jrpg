using Jrpg.Core.Content;
using Jrpg.Core.Progression;
using Jrpg.Editor.Balance;

namespace Jrpg.Editor.Tests.Balance;

// The balance table's boxes (decision 992): three typed levels, a typed damage modifier and three typed Powers, read
// into the class skill rows, the enemy skill rows, the class rows and the tier rows, each list empty while any box
// can't be read.
public sealed class BalanceTableViewModelTests
{
    private const int MaxLevel = 30;

    private const string FirstLevel = nameof(BalanceTableViewModel.FirstLevelText);
    private const string SecondLevel = nameof(BalanceTableViewModel.SecondLevelText);
    private const string ThirdLevel = nameof(BalanceTableViewModel.ThirdLevelText);
    private const string ModifierPercent = nameof(BalanceTableViewModel.ModifierPercentText);
    private const string FirstPower = nameof(BalanceTableViewModel.FirstPowerText);
    private const string SecondPower = nameof(BalanceTableViewModel.SecondPowerText);
    private const string ThirdPower = nameof(BalanceTableViewModel.ThirdPowerText);

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

    [Fact]
    public void New_PowerBoxes_StartAtOneTenAndFifty()
    {
        var table = new BalanceTableViewModel(_world);

        Assert.Equal(("1", "10", "50"), (table.FirstPowerText, table.SecondPowerText, table.ThirdPowerText));
    }

    // fire_1 gives 2 per Power; 50 % more is 500 permille on the modifiers' sum.
    [Fact]
    public void TierRows_PowersTyped_ReadAtThosePowers()
    {
        var table = new BalanceTableViewModel(_world);

        Type(table, FirstPower, "2");
        Type(table, SecondPower, "3");
        Type(table, ThirdPower, "40");
        Type(table, ModifierPercent, "50");

        Assert.Equal([6, 9, 120], Assert.Single(table.TierRows, row => row.TierId == "fire_1").Amounts);
    }

    [Theory]
    [InlineData(FirstPower, "ten", "Power 1")]
    [InlineData(SecondPower, "ten", "Power 2")]
    [InlineData(ThirdPower, "ten", "Power 3")]
    [InlineData(FirstPower, "-1", "Power 1")]
    [InlineData(FirstPower, "10001", "Power 1")]
    [InlineData(SecondPower, "-1", "Power 2")]
    [InlineData(SecondPower, "10001", "Power 2")]
    [InlineData(ThirdPower, "-1", "Power 3")]
    [InlineData(ThirdPower, "10001", "Power 3")]
    public void BoxesProblem_PowerRefused_NamesTheBoxAndEmptiesTheRows(string box, string text, string label)
    {
        var table = new BalanceTableViewModel(_world);

        Type(table, box, text);

        Assert.Equal($"{label} must be a whole number from 0 to 10000.", table.BoxesProblem);
        AssertBoxRowsEmpty(table);
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

    // The imp's Fireball is 40 at its Power 20; 50 % more gives 60.
    [Fact]
    public void EnemySkillRows_ModifierTyped_PassedAsPermille()
    {
        var table = new BalanceTableViewModel(_world);

        Type(table, ModifierPercent, "50");

        Assert.Equal("60", table.EnemySkillRows.Single(row => row.EnemyName == "Imp" && row.SkillName == "Fireball").AmountText);
    }

    // The mage at level 2: class HP 110 raised 6 % by Constitution 6, 116.6 rounded down, and Power 15.
    [Fact]
    public void ClassRows_LevelTyped_ReadAtThatLevel()
    {
        var table = new BalanceTableViewModel(_world);

        Type(table, FirstLevel, "2");

        Assert.Equal("116 / 15", table.ClassRows.Single(row => row.ClassName == "Mage").HpAndPowerTexts[0]);
    }

    [Fact]
    public void EnemyRows_LevelRefused_StillListed()
    {
        var table = new BalanceTableViewModel(_world);

        Type(table, FirstLevel, "0");

        Assert.Equal(["Bat", "Imp"], table.EnemyRows.Select(row => row.EnemyName));
    }

    [Fact]
    public void BoxesProblem_LevelPastTheMax_SaysSoAndEmptiesTheRows()
    {
        var table = new BalanceTableViewModel(_world);

        Type(table, SecondLevel, "31");

        Assert.Equal("Level 2 must be a whole number from 1 to 30.", table.BoxesProblem);
        AssertBoxRowsEmpty(table);
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
    [InlineData(FirstPower, "0")]
    [InlineData(FirstPower, "10000")]
    [InlineData(SecondPower, "0")]
    [InlineData(SecondPower, "10000")]
    [InlineData(ThirdPower, "0")]
    [InlineData(ThirdPower, "10000")]
    public void BoxesProblem_BoxAtABound_NoneAndRowsListed(string box, string text)
    {
        var table = new BalanceTableViewModel(_world);

        Type(table, box, text);

        Assert.Null(table.BoxesProblem);
        Assert.NotEmpty(table.ClassSkillRows);
        Assert.NotEmpty(table.EnemySkillRows);
        Assert.NotEmpty(table.ClassRows);
        Assert.NotEmpty(table.TierRows);
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
        AssertBoxRowsEmpty(table);
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
        AssertBoxRowsEmpty(table);
    }

    [Theory]
    [InlineData(FirstLevel)]
    [InlineData(SecondLevel)]
    [InlineData(ThirdLevel)]
    [InlineData(ModifierPercent)]
    [InlineData(FirstPower)]
    [InlineData(SecondPower)]
    [InlineData(ThirdPower)]
    public void TypedBox_Changed_RaisesRowsAndProblem(string box)
    {
        var table = new BalanceTableViewModel(_world);
        var raised = new List<string?>();
        table.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        Type(table, box, "5");

        Assert.Contains(nameof(BalanceTableViewModel.ClassSkillRows), raised);
        Assert.Contains(nameof(BalanceTableViewModel.EnemySkillRows), raised);
        Assert.Contains(nameof(BalanceTableViewModel.ClassRows), raised);
        Assert.Contains(nameof(BalanceTableViewModel.TierRows), raised);
        Assert.Contains(nameof(BalanceTableViewModel.BoxesProblem), raised);
    }

    // The workspace refuses leaving a page with a problem; the boxes' line is the page's own.
    [Fact]
    public void Problem_LevelRefused_StaysNull()
    {
        var table = new BalanceTableViewModel(_world);

        Type(table, FirstLevel, "0");

        Assert.Null(table.Problem);
    }

    // The rows read through the boxes; the enemy rows read none.
    private static void AssertBoxRowsEmpty(BalanceTableViewModel table)
    {
        Assert.Empty(table.ClassSkillRows);
        Assert.Empty(table.EnemySkillRows);
        Assert.Empty(table.ClassRows);
        Assert.Empty(table.TierRows);
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
            case FirstPower:
                table.FirstPowerText = text;
                break;
            case SecondPower:
                table.SecondPowerText = text;
                break;
            case ThirdPower:
                table.ThirdPowerText = text;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(box), box, "Not one of the table's boxes.");
        }
    }
}
