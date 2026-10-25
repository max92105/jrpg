using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A rogue with Ready (a quick skill: 1 readiness of 3) and a Focus gaining 1 a turn, then goblins A and B; the shuffle
// (2, 1) keeps the rogue first, then A, then B.
public sealed class BattleReadyTests
{
    private static readonly ResourceDefinition _readiness = new("readiness", "Readiness", Max: 3, StartsAt: 3);
    private static readonly ResourceDefinition _focus = new("focus", "Focus", Max: 5, StartsAt: 0) { Gains = new ResourceGains { PerTurn = 1 } };

    private static readonly SkillDefinition _ready = new()
    {
        Id = "ready",
        Name = "Ready",
        Description = "",
        IconFile = "",
        IsQuick = true,
        ResourceChanges = [new ResourceChange("readiness", -1)],
        Effect = new Effect(EffectKind.ReadyExtraTurn, 0, Targeting.OtherCombatant),
    };

    private readonly Combatant _rogue = Combatant.FromPartyMember(PartyMember.Recruit(
        new PartyMemberBuilder().WithId("lazarus").BuildDefinition(),
        new ClassBuilder().WithId("rogue").WithResources(_readiness, _focus).WithSkills("ready").Build(),
        new Dictionary<string, SkillDefinition> { ["ready"] = _ready }));

    private readonly Combatant _goblinA = Goblin();
    private readonly Combatant _goblinB = Goblin();
    private readonly BattleState _battle;

    public BattleReadyTests() =>
        _battle = new BattleState([_rogue, _goblinA, _goblinB], new Inventory(), new FixedSequenceRandom(2, 1));

    [Fact]
    public void Execute_Ready_ReportsAndSchedules()
    {
        var events = _battle.Execute(new SkillCommand(_ready, [_goblinA]));

        Assert.Contains(new ExtraTurnReadied(_rogue, _goblinA), events);
        Assert.DoesNotContain(events, battleEvent => battleEvent is EffectApplied);
        Assert.Equal([_goblinA, _rogue, _goblinB], _battle.UpcomingActors(3));
    }

    [Fact]
    public void Execute_ExtraTurn_NoUpkeep()
    {
        _battle.Execute(new SkillCommand(_ready, [_goblinA]));
        _battle.Execute(new DefendCommand());
        _battle.Execute(new DefendCommand());
        var focusBeforeExtraTurn = _rogue.Resources.AmountOf("focus");

        _battle.Execute(new DefendCommand());

        Assert.Equal((1, 1), (focusBeforeExtraTurn, _rogue.Resources.AmountOf("focus")));
        Assert.Same(_goblinB, _battle.CurrentActor);
    }

    [Fact]
    public void CanUse_ReadyWhilePending_ReturnsFalse()
    {
        var usableAtStart = _battle.CanUse(_ready);

        _battle.Execute(new SkillCommand(_ready, [_goblinA]));

        Assert.True(usableAtStart);
        Assert.False(_battle.CanUse(_ready));
    }

    [Fact]
    public void Execute_ReadyOnSelf_Throws()
    {
        Assert.Throws<ArgumentException>(() => _battle.Execute(new SkillCommand(_ready, [_rogue])));
    }

    // The rogue readies after goblin A, then is stunned before A acts: the extra turn is lost to the stun.
    [Fact]
    public void Execute_StunnedOnExtraTurn_TurnSkipped()
    {
        _battle.Execute(new SkillCommand(_ready, [_goblinA]));
        _battle.Execute(new DefendCommand());
        _rogue.Statuses.Apply(new StatusGrant(StatusKind.Stun, Power: 0, Turns: 2));
        _rogue.Statuses.ClearFreshMarks();

        var events = _battle.Execute(new DefendCommand());

        Assert.Contains(new TurnSkipped(_rogue, StatusKind.Stun), events);
        Assert.Same(_goblinB, _battle.CurrentActor);
        Assert.Equal(2, _rogue.Statuses.Active[0].TurnsLeft);
    }

    private static Combatant Goblin() =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 40, attack: 20, defense: 2).Build());
}
