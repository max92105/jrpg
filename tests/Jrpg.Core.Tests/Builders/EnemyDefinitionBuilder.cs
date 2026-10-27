using Jrpg.Core.Battle;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Builders;

internal sealed class EnemyDefinitionBuilder
{
    private string _name = "Goblin";
    private int _maxHp = 40;
    private int _attack = 20;
    private int _defense = 2;
    private DamageType _damageType = DamageType.Blunt;
    private Resistances _resistances = Resistances.None;

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

    public EnemyDefinitionBuilder WithDamageType(DamageType damageType)
    {
        _damageType = damageType;
        return this;
    }

    public EnemyDefinitionBuilder WithResistances(Resistances resistances)
    {
        _resistances = resistances;
        return this;
    }

    public EnemyDefinition Build() => new()
    {
        Id = _name.ToLowerInvariant(),
        Name = _name,
        PortraitFile = "goblin.png",
        BattleSpriteFile = "Goblin.png",
        AttackEffectFile = "AttackEnemy.png",
        IsSpriteFlipped = false,
        MaxHp = _maxHp,
        Attack = _attack,
        Defense = _defense,
        DamageType = _damageType,
        Resistances = _resistances,
        ExperienceReward = 40,
    };
}
