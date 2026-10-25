using Jrpg.Core.Battle;
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
        Attack = 20,
        Defense = 2,
        ExperienceReward = 40,
    };

    public static TheoryData<EnemyDefinition, string> InvalidEnemies => new()
    {
        { _goblin with { Name = "" }, "name must not be empty." },
        { _goblin with { MaxHp = 0 }, "maxHp must be at least 1 (was 0)." },
        { _goblin with { Attack = -1 }, "attack must be at least 0 (was -1)." },
        { _goblin with { Defense = -1 }, "defense must be at least 0 (was -1)." },
        { _goblin with { ExperienceReward = -1 }, "experienceReward must be at least 0 (was -1)." },
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
