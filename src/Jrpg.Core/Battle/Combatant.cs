using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;

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

    /// <summary>Fixed for the battle: equipment can't change mid-battle.</summary>
    public CombatStats Stats { get; }

    public Vitals Vitals { get; }

    public bool IsAlive => Vitals.IsAlive;

    /// <summary>The skills this combatant can use; enemies have none yet.</summary>
    public IReadOnlyList<SkillDefinition> Skills { get; private init; } = [];

    /// <summary>Built new for every battle, so each one starts afresh; enemies have none.</summary>
    public ResourceMeters Resources { get; private init; } = new([]);

    /// <summary>The party member this combatant stands for; null for an enemy. Victory experience goes to them.</summary>
    public PartyMember? Member { get; private init; }

    /// <summary>The enemy type this combatant is; null for a party member. Victory experience comes from it.</summary>
    public EnemyDefinition? Enemy { get; private init; }

    /// <summary>Shares the member's <see cref="Vitals"/>, so damage and healing in battle stay after it.</summary>
    public static Combatant FromPartyMember(PartyMember member) =>
        new(member.Definition.Name, BattleSide.Party, member.TotalStats, member.Vitals)
        {
            Skills = member.Skills,
            Resources = new(member.Class.Resources),
            Member = member,
        };

    public static Combatant FromEnemy(EnemyDefinition enemy) => new(
        enemy.Name,
        BattleSide.Enemies,
        new CombatStats(enemy.Attack, enemy.Defense),
        new Vitals(enemy.MaxHp, enemy.MaxHp, enemy.MaxMp, enemy.MaxMp))
    {
        Enemy = enemy,
    };
}
