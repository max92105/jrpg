using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// An enemy with tactics (ADR 0034): one weighted pick among what it can use, then the targets (decisions 400, 403).
public sealed class EnemySkillTacticsTests
{
    private static readonly SkillDefinition _cleave = Skill("cleave", new Effect(EffectKind.Damage, 10, Targeting.AllEnemies));
    private static readonly SkillDefinition _strike = Skill("strike", new Effect(EffectKind.Damage, 7, Targeting.SingleEnemy));
    private static readonly SkillDefinition _rally = Skill("rally", new Effect(EffectKind.GrantStatus, 0, Targeting.SingleAlly, new StatusGrant(StatusKind.Guarded, Power: 20, Turns: 2)));
    private static readonly SkillDefinition _revive = Skill("revive", new Effect(EffectKind.Revive, 10, Targeting.SingleFallenAlly));
    private static readonly SkillDefinition _brace = Skill("brace", new Effect(EffectKind.GrantStatus, 0, Targeting.Self, new StatusGrant(StatusKind.Guarded, Power: 20, Turns: 2)));
    private static readonly Dictionary<string, SkillDefinition> _skills = new[] { _cleave, _strike, _rally, _revive, _brace }.ToDictionary(skill => skill.Id);

    private static readonly EnemyAction _attack = new() { Weight = 1 };

    private readonly Combatant _sarxian = PartyCombatant("sarxian");
    private readonly Combatant _jazog = PartyCombatant("jazog");
    private readonly Combatant _teelle = PartyCombatant("teelle");

    [Fact]
    public void ChooseCommand_WeightedDrawInSkillsShare_UsesTheSkill()
    {
        var battle = EnemysTurn(Enemy(_attack, Action(_cleave, weight: 3)));

        var command = Tactics(1).ChooseCommand(battle);

        Assert.Equal(_cleave, Assert.IsType<SkillCommand>(command).Skill);
    }

    [Fact]
    public void ChooseCommand_WeightedDrawInAttacksShare_Attacks()
    {
        var battle = EnemysTurn(Enemy(_attack, Action(_cleave, weight: 3)));

        var command = Tactics(0, 1).ChooseCommand(battle);

        Assert.Equal(new AttackCommand(_jazog), command);
    }

    // The one value is the target's: a pick with one action holding the whole weight draws nothing.
    [Fact]
    public void ChooseCommand_OnlyActionIsASkill_DrawsNothingForThePick()
    {
        var battle = EnemysTurn(Enemy(Action(_strike, weight: 2)));

        var command = Tactics(1).ChooseCommand(battle);

        Assert.Equal([_jazog], Assert.IsType<SkillCommand>(command).Targets);
    }

    // Nobody has fallen, so the revive is not eligible: the attack holds the whole weight; the one value is its target.
    [Fact]
    public void ChooseCommand_SkillWithNoTarget_IsNotPicked()
    {
        var battle = EnemysTurn(Enemy(_attack, Action(_revive, weight: 3)));

        var command = Tactics(1).ChooseCommand(battle);

        Assert.Equal(new AttackCommand(_jazog), command);
    }

    [Fact]
    public void ChooseCommand_OnlySkillUnusable_Attacks()
    {
        var battle = EnemysTurn(Enemy(Action(_revive, weight: 3)));

        var command = Tactics(2).ChooseCommand(battle);

        Assert.Equal(new AttackCommand(_teelle), command);
    }

    [Fact]
    public void ChooseCommand_AllEnemiesSkill_TargetsEveryLivingMember()
    {
        _jazog.Vitals.TakeDamage(999);
        var battle = EnemysTurn(Enemy(Action(_cleave, weight: 1)));

        var command = Tactics().ChooseCommand(battle);

        Assert.Equal([_sarxian, _teelle], Assert.IsType<SkillCommand>(command).Targets);
    }

    [Fact]
    public void ChooseCommand_SingleEnemySkillWithATaunter_AimsAtTheTaunter()
    {
        Taunt(_jazog);
        var battle = EnemysTurn(Enemy(Action(_strike, weight: 1)));

        var command = Tactics(0).ChooseCommand(battle);

        Assert.Equal([_jazog], Assert.IsType<SkillCommand>(command).Targets);
    }

    // The roll under 50 % shrugs the taunt off, so the second draw picks among all three: index 2 is Teelle.
    [Fact]
    public void ChooseCommand_SingleEnemySkillTauntResisted_AimsAtAnyMember()
    {
        Taunt(_jazog);
        var battle = EnemysTurn(new EnemyDefinitionBuilder().WithCrowdControlResistance(50).WithTactics(Action(_strike, weight: 1)).Build());

        var command = Tactics(499, 2).ChooseCommand(battle);

        Assert.Equal([_teelle], Assert.IsType<SkillCommand>(command).Targets);
    }

    // A skill that takes every target ignores a taunt (decision 400).
    [Fact]
    public void ChooseCommand_AllEnemiesSkillWithATaunter_TargetsEveryMember()
    {
        Taunt(_jazog);
        var battle = EnemysTurn(Enemy(Action(_cleave, weight: 1)));

        var command = Tactics().ChooseCommand(battle);

        Assert.Equal([_sarxian, _jazog, _teelle], Assert.IsType<SkillCommand>(command).Targets);
    }

    // The enemy's allies are the enemies, itself included: the draw picks among those two and never a member.
    [Theory]
    [InlineData(0, "Second")]
    [InlineData(1, "First")]
    public void ChooseCommand_SingleAllySkill_AimsAtARandomEnemy(int draw, string expectedName)
    {
        var actor = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithName("First").WithTactics(Action(_rally, weight: 1)).Build());
        var ally = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithName("Second").Build());
        // Shuffle values (0, 1) swap the last combatant to the front, so the first enemy acts.
        var battle = new BattleState([_sarxian, ally, actor], new Inventory(), new FixedSequenceRandom(0, 1));

        var command = Tactics(draw).ChooseCommand(battle);

        Assert.Equal(expectedName, Assert.Single(Assert.IsType<SkillCommand>(command).Targets).Name);
    }

    // One candidate is a certain outcome, so nothing is drawn (ADR 0026).
    [Fact]
    public void ChooseCommand_SelfSkill_AimsAtItselfWithoutADraw()
    {
        var battle = EnemysTurn(Enemy(Action(_brace, weight: 1)));

        var command = Tactics().ChooseCommand(battle);

        Assert.Equal([battle.CurrentActor], Assert.IsType<SkillCommand>(command).Targets);
    }

    // Decision 413: an enemy's stats scale by 1000 permille, and it has no resource to spend or gain.
    [Fact]
    public void Execute_EnemySkillCommand_LandsTheSkillsContentPower()
    {
        var battle = EnemysTurn(Enemy(Action(_strike, weight: 1)));

        var events = battle.Execute(new SkillCommand(_strike, [_jazog]));

        var applied = Assert.Single(events.OfType<EffectApplied>());
        Assert.Equal(_jazog, applied.Target);
        Assert.Equal(7, applied.Amount);
        Assert.Empty(events.OfType<ResourceChanged>());
    }

    private static EnemyTactics Tactics(params int[] draws) => new(new FixedSequenceRandom(draws), _skills);

    // Shuffle values (0, 2, 1) swap the enemy to the front, so it is the enemy's turn.
    private BattleState EnemysTurn(EnemyDefinition enemy) =>
        new([_sarxian, _jazog, _teelle, Combatant.FromEnemy(enemy)], new Inventory(), new FixedSequenceRandom(0, 2, 1));

    private static EnemyDefinition Enemy(params EnemyAction[] actions) => new EnemyDefinitionBuilder().WithTactics(actions).Build();

    private static EnemyAction Action(SkillDefinition skill, int weight) => new() { SkillId = skill.Id, Weight = weight };

    private static SkillDefinition Skill(string id, Effect effect) => new() { Id = id, Name = id, Description = "", IconFile = "", Effect = effect };

    private static void Taunt(Combatant member) => member.Statuses.Apply(new StatusGrant(StatusKind.Taunt, Power: 0, Turns: 2));

    private static Combatant PartyCombatant(string id) => Combatant.FromPartyMember(new PartyMemberBuilder().WithId(id).Build());
}
