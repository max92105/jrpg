using System.Collections.Frozen;
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

    // Listed against id order, and named against it, so a sort by name would fail.
    [Fact]
    public void LearnedBy_TwoClasses_NamesBothInIdOrder()
    {
        var warlock = _world.Classes["mage"] with { Id = "warlock", Name = "Warlock", SkillIds = ["bolt"] };
        var mage = _world.Classes["mage"] with { Name = "Wizard", SkillIds = ["zap", "bolt"] };
        var content = _world with { Classes = ClassesOf(warlock, mage) };

        var line = SkillUsers.LearnedBy("bolt", content);

        Assert.Equal("Learned by: Wizard, Warlock", line);
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

    // Given against id order and named against it, so a sort by name or the given order would fail.
    [Fact]
    public void UsedBy_TwoEnemies_NamesBothInIdOrder()
    {
        var tactics = new[] { new EnemyAction { SkillId = "bolt", Weight = 1 } };
        var wolf = _world.Enemies["goblin"] with { Id = "wolf", Name = "Alpha", Tactics = tactics, Phases = [] };
        var bat = _world.Enemies["goblin"] with { Id = "bat", Name = "Zubat", Tactics = tactics, Phases = [] };

        var line = SkillUsers.UsedBy("bolt", _world with { Enemies = EnemiesOf(wolf, bat) });

        Assert.Equal("Used by: Zubat, Alpha", line);
    }

    [Fact]
    public void UsedBy_NoEnemy_IsEmpty()
    {
        var goblin = _world.Enemies["goblin"] with { Tactics = [new EnemyAction { SkillId = "zap", Weight = 1 }], Phases = [] };

        var line = SkillUsers.UsedBy("bolt", _world with { Enemies = EnemiesOf(goblin) });

        Assert.Equal("", line);
    }

    // Frozen, as the loader keeps them: its order isn't the order the classes were given in.
    private static FrozenDictionary<string, ClassDefinition> ClassesOf(params ClassDefinition[] classes) =>
        classes.ToFrozenDictionary(characterClass => characterClass.Id, StringComparer.Ordinal);

    private static FrozenDictionary<string, EnemyDefinition> EnemiesOf(params EnemyDefinition[] enemies) =>
        enemies.ToFrozenDictionary(enemy => enemy.Id, StringComparer.Ordinal);
}
