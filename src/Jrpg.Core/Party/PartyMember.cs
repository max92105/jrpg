using Jrpg.Core.Progression;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Party;

/// <summary>A recruited party member's runtime state.</summary>
public sealed class PartyMember
{
    private PartyMember(PartyMemberDefinition definition)
    {
        Definition = definition;
        Level = definition.Level;
        Experience = definition.Experience;
        BaseStats = new CombatStats(definition.Attack, definition.Defense);
        Vitals = new Vitals(definition.Hp, definition.MaxHp, definition.Mp, definition.MaxMp);
    }

    public string Id => Definition.Id;

    public PartyMemberDefinition Definition { get; }

    public int Level { get; private set; }

    /// <summary>Experience earned toward the next level; resets to 0 on level-up.</summary>
    public int Experience { get; private set; }

    public CombatStats BaseStats { get; }

    public Vitals Vitals { get; }

    public static PartyMember Recruit(PartyMemberDefinition definition) => new(definition);

    /// <returns>One step per level the experience passes through; empty at the maximum level.</returns>
    public IReadOnlyList<LevelProgressStep> GainExperience(int amount, ExperienceTable table)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        var steps = new List<LevelProgressStep>();
        var remaining = amount;
        while (remaining > 0 && !table.IsMaxLevel(Level))
        {
            var step = AdvanceTowardNextLevel(remaining, table.ExperienceToReachNext(Level));
            remaining -= step.ToExperience - step.FromExperience;
            steps.Add(step);
        }

        return steps;
    }

    private LevelProgressStep AdvanceTowardNextLevel(int amount, int experienceToNextLevel)
    {
        var from = Experience;
        Experience += Math.Min(amount, experienceToNextLevel - from);
        var step = new LevelProgressStep(Level, from, Experience, experienceToNextLevel);

        if (step.IsLevelUp)
        {
            // Placeholder: a level-up grants no stat growth until classes are designed (plan 2026-10-17, guiding rule).
            Level++;
            Experience = 0;
        }

        return step;
    }
}
