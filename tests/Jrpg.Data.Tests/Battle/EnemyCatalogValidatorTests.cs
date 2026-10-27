using Jrpg.Core.Battle;
using Jrpg.Core.Stats;
using Jrpg.Data.Battle;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Battle;

public sealed class EnemyCatalogValidatorTests
{
    private static readonly EnemyDefinition _goblin = new()
    {
        Id = "goblin",
        Name = "Goblin",
        PortraitFile = "goblin.png",
        BattleSpriteFile = "Goblin.png",
        AttackEffectFile = "AttackEnemy.png",
        IsSpriteFlipped = false,
        MaxHp = 40,
        Damage = 20,
        DamageType = DamageType.Piercing,
        Accuracy = 85,
        Dodge = 5,
        ExperienceReward = 40,
    };

    public static TheoryData<EnemyDefinition, string> InvalidEnemies => new()
    {
        { _goblin with { Name = "" }, "name must not be empty." },
        { _goblin with { MaxHp = 0 }, "maxHp must be at least 1 (was 0)." },
        { _goblin with { Damage = -1 }, "damage must be at least 0 (was -1)." },
        { _goblin with { Accuracy = 201 }, "accuracy must be within 0..200 (was 201)." },
        { _goblin with { Dodge = 51 }, "dodge must be within 0..50 (was 51)." },
        { _goblin with { ExperienceReward = -1 }, "experienceReward must be at least 0 (was -1)." },
        { _goblin with { Resistances = Resistances.None with { Fire = 760 } }, "resistances.fire must be within -100..75 (was 76)." },
        { _goblin with { Resistances = Resistances.None with { Ice = -1010 } }, "resistances.ice must be within -100..75 (was -101)." },
    };

    [Fact]
    public void Validate_ValidEnemy_DoesNotThrow()
    {
        var exception = Record.Exception(() => EnemyCatalogValidator.Validate("enemies.json", [_goblin]));

        Assert.Null(exception);
    }

    [Theory]
    [MemberData(nameof(InvalidEnemies))]
    public void Validate_InvalidField_ThrowsNamingTheEnemyAndField(EnemyDefinition enemy, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(() => EnemyCatalogValidator.Validate("enemies.json", [enemy]));

        Assert.Equal($"enemies.json: enemy 'goblin': {problem}", exception.Message);
    }
}
