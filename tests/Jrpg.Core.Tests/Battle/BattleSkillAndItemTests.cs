using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// The scripted shuffle (3, 2, 1) keeps the four combatants in listed order, so the mage acts first.
public sealed class BattleSkillAndItemTests
{
    private static readonly SkillDefinition _fireball = Skill("fireball", mpCost: 10, new Effect(EffectKind.Damage, 20, Targeting.AllEnemies));
    private static readonly SkillDefinition _heal = Skill("heal", mpCost: 8, new Effect(EffectKind.RestoreHp, 15, Targeting.SingleAlly));

    private readonly Combatant _mage = PartyCombatant("mage", hp: 100, mp: 30, _fireball, _heal);
    private readonly Combatant _ally = PartyCombatant("ally", hp: 50, mp: 0);
    private readonly Combatant _goblin = Goblin();
    private readonly Combatant _otherGoblin = Goblin();
    private readonly Inventory _inventory = new();

    [Fact]
    public void Execute_SkillCommand_SpendsMpAndHitsEveryTarget()
    {
        var battle = CreateBattle();
        var fireball = new SkillCommand(_fireball, [_goblin, _otherGoblin]);

        var events = battle.Execute(fireball);

        BattleEvent[] expected =
        [
            new ActionStarted(_mage, fireball),
            new EffectApplied(_goblin, EffectKind.Damage, 18),
            new EffectApplied(_otherGoblin, EffectKind.Damage, 18),
        ];
        Assert.Equal(expected, events);
        Assert.Equal(20, _mage.Vitals.Mp);
    }

    [Fact]
    public void Execute_AllEnemiesSkillMissingAnEnemy_ThrowsAndChangesNothing()
    {
        var battle = CreateBattle();

        Assert.Throws<ArgumentException>(() => battle.Execute(new SkillCommand(_fireball, [_goblin])));

        Assert.Equal(30, _mage.Vitals.Mp);
        Assert.Equal(_goblin.Vitals.MaxHp, _goblin.Vitals.Hp);
        Assert.Same(_mage, battle.CurrentActor);
    }

    [Fact]
    public void Execute_SkillCommandWithoutEnoughMp_ThrowsAndChangesNothing()
    {
        _mage.Vitals.TrySpendMp(25);
        var battle = CreateBattle();

        Assert.False(battle.CanUse(_fireball));
        Assert.Throws<InvalidOperationException>(() => battle.Execute(new SkillCommand(_fireball, [_goblin, _otherGoblin])));
        Assert.Equal(5, _mage.Vitals.Mp);
    }

    [Fact]
    public void Execute_SkillTheActorDoesNotKnow_Throws()
    {
        var unknown = Skill("thunder", mpCost: 0, new Effect(EffectKind.Damage, 5, Targeting.SingleEnemy));
        var battle = CreateBattle();

        Assert.Throws<InvalidOperationException>(() => battle.Execute(new SkillCommand(unknown, [_goblin])));
    }

    [Fact]
    public void Execute_SingleTargetSkillOnTwoTargets_Throws()
    {
        var battle = CreateBattle();

        Assert.Throws<ArgumentException>(() => battle.Execute(new SkillCommand(_heal, [_mage, _ally])));
    }

    [Fact]
    public void Execute_HealOnFallenAlly_Throws()
    {
        _ally.Vitals.TakeDamage(999);
        var battle = CreateBattle();

        Assert.Throws<ArgumentException>(() => battle.Execute(new SkillCommand(_heal, [_ally])));
    }

    [Fact]
    public void Execute_ItemCommand_ConsumesOneCopy()
    {
        var potion = Item("potion", new Effect(EffectKind.RestoreHp, 15, Targeting.SingleAlly));
        _inventory.Add(potion, 2);
        var battle = CreateBattle();
        var usePotion = new ItemCommand(potion, [_ally]);

        var events = battle.Execute(usePotion);

        Assert.Equal([new ActionStarted(_mage, usePotion), new EffectApplied(_ally, EffectKind.RestoreHp, 15)], events);
        Assert.Equal(65, _ally.Vitals.Hp);
        Assert.Equal(1, _inventory.QuantityOf(potion.Id));
    }

    [Fact]
    public void Execute_ItemCommandOnInvalidTarget_ThrowsAndKeepsTheCopy()
    {
        var potion = Item("potion", new Effect(EffectKind.RestoreHp, 15, Targeting.SingleAlly));
        _inventory.Add(potion, 1);
        var battle = CreateBattle();

        Assert.Throws<ArgumentException>(() => battle.Execute(new ItemCommand(potion, [_goblin])));

        Assert.Equal(1, _inventory.QuantityOf(potion.Id));
    }

    [Fact]
    public void Execute_ItemNotOwned_Throws()
    {
        var potion = Item("potion", new Effect(EffectKind.RestoreHp, 15, Targeting.SingleAlly));
        var battle = CreateBattle();

        Assert.False(battle.CanUse(potion));
        Assert.Throws<InvalidOperationException>(() => battle.Execute(new ItemCommand(potion, [_ally])));
    }

    [Fact]
    public void Execute_ReviveItemOnFallenAlly_RevivesThem()
    {
        var feather = Item("revive_feather", new Effect(EffectKind.Revive, 15, Targeting.AllFallenAllies));
        _inventory.Add(feather, 1);
        _ally.Vitals.TakeDamage(999);
        var battle = CreateBattle();

        var useFeather = new ItemCommand(feather, [_ally]);

        var events = battle.Execute(useFeather);

        Assert.Equal([new ActionStarted(_mage, useFeather), new EffectApplied(_ally, EffectKind.Revive, 15)], events);
        Assert.True(_ally.IsAlive);
        Assert.Equal(0, _inventory.QuantityOf(feather.Id));
    }

    [Fact]
    public void Execute_NonReviveEffectOnFallenAlly_DoesNotReportDefeatAgain()
    {
        var tonic = Item("tonic", new Effect(EffectKind.RestoreHp, 15, Targeting.SingleFallenAlly));
        _inventory.Add(tonic, 1);
        _ally.Vitals.TakeDamage(999);
        var battle = CreateBattle();
        var useTonic = new ItemCommand(tonic, [_ally]);

        var events = battle.Execute(useTonic);

        Assert.Equal([new ActionStarted(_mage, useTonic), new EffectApplied(_ally, EffectKind.RestoreHp, 0)], events);
    }

    [Fact]
    public void Execute_AllEnemiesSkillWithDuplicateTarget_ThrowsAndChangesNothing()
    {
        var battle = CreateBattle();

        Assert.Throws<ArgumentException>(() => battle.Execute(new SkillCommand(_fireball, [_goblin, _goblin])));

        Assert.Equal(30, _mage.Vitals.Mp);
        Assert.Equal(_goblin.Vitals.MaxHp, _goblin.Vitals.Hp);
    }

    [Fact]
    public void Execute_ItemOnEnemyTurn_ThrowsAndKeepsTheCopy()
    {
        var potion = Item("potion", new Effect(EffectKind.RestoreHp, 15, Targeting.SingleAlly));
        _inventory.Add(potion, 1);
        // Shuffle (0, 2, 1) swaps the second goblin to the front, so it is an enemy's turn.
        var battle = new BattleState([_mage, _ally, _goblin, _otherGoblin], _inventory, new FixedSequenceRandom(0, 2, 1));

        Assert.False(battle.CanUse(potion));
        Assert.Throws<InvalidOperationException>(() => battle.Execute(new ItemCommand(potion, [_goblin])));
        Assert.Equal(1, _inventory.QuantityOf(potion.Id));
    }

    [Fact]
    public void Execute_ItemWithoutEffect_Throws()
    {
        var sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(5, "warrior").Build();
        _inventory.Add(sword, 1);
        var battle = CreateBattle();

        Assert.False(battle.CanUse(sword));
        Assert.Throws<InvalidOperationException>(() => battle.Execute(new ItemCommand(sword, [_goblin])));
    }

    [Fact]
    public void Execute_DamageSkillFellsLastEnemies_ReportsEachDefeatThenVictory()
    {
        var weakGoblin = Goblin(maxHp: 10);
        var otherWeakGoblin = Goblin(maxHp: 10);
        var battle = new BattleState([_mage, _ally, weakGoblin, otherWeakGoblin], _inventory, new FixedSequenceRandom(3, 2, 1));

        var events = battle.Execute(new SkillCommand(_fireball, [weakGoblin, otherWeakGoblin]));

        Assert.Equal(
            [typeof(ActionStarted), typeof(EffectApplied), typeof(CombatantDefeated), typeof(EffectApplied), typeof(CombatantDefeated), typeof(BattleEnded)],
            events.Select(battleEvent => battleEvent.GetType()));
        Assert.Equal(BattlePhase.Victory, battle.Phase);
    }

    private static SkillDefinition Skill(string id, int mpCost, Effect effect) =>
        new() { Id = id, Name = id, Description = "", IconFile = "Attack.png", MpCost = mpCost, Effect = effect };

    private static ItemDefinition Item(string id, Effect effect) =>
        new ItemDefinitionBuilder().WithId(id).WithEffect(effect).Build();

    private static Combatant PartyCombatant(string id, int hp, int mp, params SkillDefinition[] skills) =>
        Combatant.FromPartyMember(new PartyMemberBuilder()
            .WithId(id)
            .WithVitals(hp, maxHp: 100, mp, maxMp: 30)
            .WithSkills(skills)
            .Build());

    private static Combatant Goblin(int maxHp = 40) =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp, attack: 20, defense: 2).Build());

    private BattleState CreateBattle() =>
        new([_mage, _ally, _goblin, _otherGoblin], _inventory, new FixedSequenceRandom(3, 2, 1));
}
