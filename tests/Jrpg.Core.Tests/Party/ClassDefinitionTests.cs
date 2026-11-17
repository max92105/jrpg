using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

public sealed class ClassDefinitionTests
{
    // Levels out of order, so ids sorted by level would differ from the entries' order, the menu's (ADR 0020).
    [Fact]
    public void SkillIds_EntriesAtSeveralLevels_ListsEveryIdInOrder()
    {
        var mage = new ClassBuilder().Build() with
        {
            Skills = [new("meteor") { Level = 5 }, new("fire_blast"), new("frost") { Level = 3 }],
        };

        Assert.Equal(["meteor", "fire_blast", "frost"], mage.SkillIds);
    }
}
