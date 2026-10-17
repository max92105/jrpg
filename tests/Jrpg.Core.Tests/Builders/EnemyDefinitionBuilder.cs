using Jrpg.Core.Battle;

namespace Jrpg.Core.Tests.Builders;

internal sealed class EnemyDefinitionBuilder
{
    private string _name = "Goblin";
    private int _maxHp = 40;
    private int _attack = 20;
    private int _defense = 2;

    public EnemyDefinitionBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public EnemyDefinitionBuilder WithStats(int maxHp, int attack, int defense)
    {
        _maxHp = maxHp;
        _attack = attack;
        _defense = defense;
        return this;
    }

    public EnemyDefinition Build() => new()
    {
        Id = _name.ToLowerInvariant(),
        Name = _name,
        PortraitFile = "goblin.png",
        BattleSpriteFile = "Goblin.png",
        AttackEffectFile = "AttackEnemy.gif",
        IsSpriteFlipped = false,
        MaxHp = _maxHp,
        MaxMp = 10,
        Attack = _attack,
        Defense = _defense,
        ExperienceReward = 40,
    };
}
