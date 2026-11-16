using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Editor.Skills;

namespace Jrpg.Editor.Tests.Skills;

// Who uses a skill (decision 978): the classes listing it and the enemies whose tactics or phases name it, built here
// over the test world's records so no repository list is pinned.
public sealed class SkillUsersTests
{
    private static readonly ContentCatalog _world = SkillFixtures.TestWorld;

    // Listed against their names' order, so a sort by name would fail.
    [Fact]
    public void LearnedBy_TwoClasses_NamesBothInClassOrder()
    {
        var warlock = _world.Classes["mage"] with { Id = "warlock", Name = "Warlock", SkillIds = ["bolt"] };
        var mage = _world.Classes["mage"] with { Name = "Mage", SkillIds = ["zap", "bolt"] };
        var content = _world with { Classes = ClassesOf(warlock, mage) };

        var line = SkillUsers.LearnedBy("bolt", content);

        Assert.Equal("Learned by: Warlock, Mage", line);
    }

    [Fact]
    public void LearnedBy_NoClass_SaysSo()
    {
        var mage = _world.Classes["mage"] with { SkillIds = ["zap"] };

        var line = SkillUsers.LearnedBy("bolt", _world with { Classes = ClassesOf(mage) });

        Assert.Equal("Learned by: no class", line);
    }

    [Fact]
    public void UsedBy_EnemyPhaseNamesIt_NamesTheEnemy()
    {
        var bolt = new EnemyAction { SkillId = "bolt", Weight = 1 };
        var chief = _world.Enemies["goblin"] with
        {
            Name = "Goblin Chief",
            Tactics = [],
            Phases = [new EnemyPhase { BelowHpPercent = 50, Tactics = [bolt] }],
        };

        var line = SkillUsers.UsedBy("bolt", _world with { Enemies = EnemiesOf(chief) });

        Assert.Equal("Used by: Goblin Chief", line);
    }

    [Fact]
    public void UsedBy_NoEnemy_IsEmpty()
    {
        var goblin = _world.Enemies["goblin"] with { Tactics = [new EnemyAction { SkillId = "zap", Weight = 1 }], Phases = [] };

        var line = SkillUsers.UsedBy("bolt", _world with { Enemies = EnemiesOf(goblin) });

        Assert.Equal("", line);
    }

    private static Dictionary<string, ClassDefinition> ClassesOf(params ClassDefinition[] classes) =>
        classes.ToDictionary(characterClass => characterClass.Id, StringComparer.Ordinal);

    private static Dictionary<string, EnemyDefinition> EnemiesOf(params EnemyDefinition[] enemies) =>
        enemies.ToDictionary(enemy => enemy.Id, StringComparer.Ordinal);
}
