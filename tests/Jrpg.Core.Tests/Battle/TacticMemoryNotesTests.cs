using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Battle;

// The events are written here as a call reports them, for what Execute can't produce today: no enemy has a block
// chance, and no enemy's action can hit its own side (decision 845).
public sealed class TacticMemoryNotesTests
{
    private readonly Combatant _member = Combatant.FromPartyMember(new PartyMemberBuilder().Build());
    private readonly Combatant _enemy = Combatant.FromEnemy(new EnemyDefinitionBuilder().Build());

    // A blocked hit is reported before its number, and still lands.
    [Fact]
    public void Note_BlockedHit_CountsAsAHit()
    {
        TacticMemoryNotes.Note(
            [new ActionStarted(_member, new AttackCommand(_enemy)), new HitBlocked(_enemy), Damage(_enemy)]);

        Assert.Equal([_member], _enemy.TacticMemory.Noted(TacticConditionKind.WasHit));
    }

    [Fact]
    public void Note_TwoHitsFromOneFoe_NotesItOnce()
    {
        TacticMemoryNotes.Note([new ActionStarted(_member, new AttackCommand(_enemy)), Damage(_enemy), Damage(_enemy)]);

        Assert.Equal([_member], _enemy.TacticMemory.Noted(TacticConditionKind.WasHit));
    }

    [Fact]
    public void Note_AnAllysHitOnAnEnemy_NotesNothing()
    {
        var ally = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithName("Ally").Build());

        TacticMemoryNotes.Note([new ActionStarted(ally, new AttackCommand(_enemy)), Damage(_enemy)]);

        Assert.Empty(_enemy.TacticMemory.Noted(TacticConditionKind.WasHit));
    }

    [Fact]
    public void Note_AHealOnAnEnemy_NotesNothing()
    {
        TacticMemoryNotes.Note(
            [new ActionStarted(_member, new DefendCommand()), new EffectApplied(_enemy, EffectKind.RestoreHp, 5)]);

        Assert.Empty(_enemy.TacticMemory.Noted(TacticConditionKind.WasHit));
    }

    private static EffectApplied Damage(Combatant target) => new(target, EffectKind.Damage, 5);
}
