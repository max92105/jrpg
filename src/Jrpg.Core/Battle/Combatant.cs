using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>One participant in a battle, on either side.</summary>
public sealed class Combatant
{
    private Combatant(string name, BattleSide side, CombatStats stats, Vitals vitals)
    {
        Name = name;
        Side = side;
        Stats = stats;
        Vitals = vitals;
    }

    public string Name { get; }

    public BattleSide Side { get; }

    /// <summary>The base for the battle, before statuses: equipment can't change mid-battle.</summary>
    public CombatStats Stats { get; }

    /// <summary>The stats with the statuses applied now; what attacks and effects read (ADR 0021).</summary>
    public CombatStats CurrentStats => StatusModifiers.Apply(Stats, Statuses);

    public Vitals Vitals { get; }

    public bool IsAlive => Vitals.IsAlive;

    /// <summary>The skills this combatant can use; enemies have none yet.</summary>
    public IReadOnlyList<SkillDefinition> Skills { get; private init; } = [];

    /// <summary>Built new for every battle, so each one starts afresh; enemies have none.</summary>
    public ResourceMeters Resources { get; private init; } = new([]);

    /// <summary>Battle only, never saved (ADR 0021).</summary>
    public StatusSet Statuses { get; } = new();

    /// <summary>The party member this combatant stands for; null for an enemy. Victory experience goes to them.</summary>
    public PartyMember? Member { get; private init; }

    /// <summary>The enemy type this combatant is; null for a party member. Victory experience comes from it.</summary>
    public EnemyDefinition? Enemy { get; private init; }

    /// <summary>Shares the member's <see cref="Vitals"/>, so damage and healing in battle stay after it.</summary>
    public static Combatant FromPartyMember(PartyMember member) =>
        new(member.Definition.Name, BattleSide.Party, MemberBattleStats.Of(member), member.Vitals)
        {
            Skills = member.Skills,
            Resources = new(ResourceStarts.For(member)),
            Member = member,
        };

    public static Combatant FromEnemy(EnemyDefinition enemy) => new(
        enemy.Name,
        BattleSide.Enemies,
        new CombatStats(enemy.Damage) { AttackType = enemy.DamageType, Resistances = enemy.Resistances },
        new Vitals(enemy.MaxHp, enemy.MaxHp))
    {
        Enemy = enemy,
    };

    /// <summary>Changes one resource within 0..max and reports it, unless clamping left nothing to change.</summary>
    internal void ChangeResource(string resourceId, int amount, List<BattleEvent> events)
    {
        var applied = Resources.Change(resourceId, amount);
        if (applied != 0)
        {
            events.Add(new ResourceChanged(this, resourceId, applied));
        }
    }
}
