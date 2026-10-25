using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A mage with 10 max mana, starting at 3 and gaining 2 at the end of each of the mage's turns, against one goblin.
// The shuffle (1) keeps the listed order: the mage, then the goblin.
public sealed class BattleStateResourceTests
{
    private static readonly ResourceDefinition _mana =
        new("mana", "Mana", Max: 10, StartsAt: 3) { Gains = new ResourceGains { PerTurn = 2 } };

    private readonly PartyMember _mage = PartyMember.Recruit(
        new PartyMemberBuilder().WithId("omer").BuildDefinition(),
        new ClassBuilder().WithId("mage").WithResources(_mana).Build(),
        new Dictionary<string, SkillDefinition>());

    [Fact]
    public void Start_MemberWithResource_StartsAtStartsAt()
    {
        var battle = StartBattle();

        Assert.Equal(3, MageIn(battle).Resources.AmountOf("mana"));
    }

    [Fact]
    public void Execute_EndOfMemberTurn_GainsPerTurnAndReportsIt()
    {
        var battle = StartBattle();
        var mage = MageIn(battle);

        var events = battle.Execute(new DefendCommand());

        Assert.Equal(5, mage.Resources.AmountOf("mana"));
        Assert.Contains(new ResourceChanged(mage, "mana", 2), events);
    }

    [Fact]
    public void Execute_AtMax_ReportsNoChange()
    {
        var battle = StartBattle();
        var mage = MageIn(battle);
        mage.Resources.Change("mana", 10);

        var events = battle.Execute(new DefendCommand());

        Assert.Equal(10, mage.Resources.AmountOf("mana"));
        Assert.DoesNotContain(events, battleEvent => battleEvent is ResourceChanged);
    }

    [Fact]
    public void Execute_EnemyTurn_MemberGainsNothing()
    {
        var battle = StartBattle();
        battle.Execute(new DefendCommand());

        var events = battle.Execute(new DefendCommand());

        Assert.Equal(5, MageIn(battle).Resources.AmountOf("mana"));
        Assert.DoesNotContain(events, battleEvent => battleEvent is ResourceChanged);
    }

    [Fact]
    public void Start_SecondBattle_StartsAtStartsAgain()
    {
        var first = StartBattle();
        first.Execute(new DefendCommand());

        var second = StartBattle();

        Assert.Equal((5, 3), (MageIn(first).Resources.AmountOf("mana"), MageIn(second).Resources.AmountOf("mana")));
    }

    private BattleState StartBattle()
    {
        var party = new PartyState([_mage], [_mage], new PartyRules(MaxActiveMembers: 4));
        var goblin = new EnemyDefinitionBuilder().WithStats(maxHp: 40, attack: 20, defense: 2).Build();
        return BattleState.Start(party, [goblin], new Inventory(), new FixedSequenceRandom(1));
    }

    private static Combatant MageIn(BattleState battle) => battle.LivingOn(BattleSide.Party)[0];
}
