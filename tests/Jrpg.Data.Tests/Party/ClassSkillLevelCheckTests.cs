using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Data.Content;
using Jrpg.Data.Party;

namespace Jrpg.Data.Tests.Party;

public sealed class ClassSkillLevelCheckTests
{
    // Two steps, so the max level is 3.
    private static readonly ExperienceTable _table = new([100, 120]);

    [Fact]
    public void Check_LevelAtMax_IsAccepted()
    {
        var exception = Record.Exception(() => Check(Mage(new ClassSkill("meteor") { Level = 3 })));

        Assert.Null(exception);
    }

    [Fact]
    public void Check_LevelPastMax_NamesClassAndSkill()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => Check(Mage(new ClassSkill("fire_blast"), new ClassSkill("meteor") { Level = 4 })));

        Assert.Equal("classes.json: class 'mage': skills lists 'meteor' at level 4, past the max level 3.", exception.Message);
    }

    private static void Check(ClassDefinition classDefinition) =>
        ClassSkillLevelCheck.Check("classes.json", [classDefinition], _table);

    private static ClassDefinition Mage(params ClassSkill[] skills) =>
        new("mage", "Mage", "Casts.", new ClassStats(100, 10), new ClassStats(0, 0)) { Skills = skills };
}
