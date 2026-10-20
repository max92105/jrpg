using Jrpg.Core.Items;
using Jrpg.Core.Progression;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Party;

/// <summary>A recruited party member's runtime state.</summary>
public sealed class PartyMember
{
    private PartyMember(PartyMemberDefinition definition, IReadOnlyList<SkillDefinition> skills, PartyMemberProgress progress)
    {
        Definition = definition;
        Skills = [.. skills];
        Level = progress.Level;
        Experience = progress.Experience;
        BaseStats = new CombatStats(definition.Attack, definition.Defense);
        Vitals = new Vitals(progress.Hp, definition.MaxHp, progress.Mp, definition.MaxMp);
    }

    public string Id => Definition.Id;

    public PartyMemberDefinition Definition { get; }

    public int Level { get; private set; }

    /// <summary>Experience earned toward the next level; resets to 0 on level-up.</summary>
    public int Experience { get; private set; }

    public CombatStats BaseStats { get; }

    public Vitals Vitals { get; }

    public ItemDefinition? Weapon { get; private set; }

    public ItemDefinition? Armor { get; private set; }

    public IReadOnlyList<SkillDefinition> Skills { get; }

    /// <summary>Base stats plus the bonuses of everything worn.</summary>
    public CombatStats TotalStats => StatsWearing(Weapon, Armor);

    /// <param name="definition">Who joins and with what starting values.</param>
    /// <param name="skillCatalog">All skills by id; the member gets those named by <see cref="PartyMemberDefinition.SkillIds"/>, in that order.</param>
    /// <exception cref="ArgumentException">A skill id isn't in the catalog.</exception>
    public static PartyMember Recruit(PartyMemberDefinition definition, IReadOnlyDictionary<string, SkillDefinition> skillCatalog)
    {
        var starting = new PartyMemberProgress(
            definition.Id, definition.Level, definition.Experience, definition.Hp, definition.Mp, WeaponId: null, ArmorId: null);
        return new(definition, SkillsOf(definition, skillCatalog), starting);
    }

    /// <summary>Rebuilds a member from a save. Worn items are put on by <c>GameSession</c>, which resolves their ids.</summary>
    /// <remarks>The progress is assumed repaired against current content by Data (ADR 0007).</remarks>
    internal static PartyMember Restore(
        PartyMemberDefinition definition, IReadOnlyDictionary<string, SkillDefinition> skillCatalog, PartyMemberProgress progress) =>
        new(definition, SkillsOf(definition, skillCatalog), progress);

    /// <summary>What a save keeps of this member.</summary>
    public PartyMemberProgress CaptureProgress() =>
        new(Id, Level, Experience, Vitals.Hp, Vitals.Mp, Weapon?.Id, Armor?.Id);

    /// <summary>The stat preview of taking off what's worn in <paramref name="slot"/>: total stats with that slot empty.</summary>
    public CombatStats StatsWithout(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Weapon => StatsWearing(null, Armor),
        EquipmentSlot.Armor => StatsWearing(Weapon, null),
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown equipment slot."),
    };

    /// <summary>The stat preview: total stats if <paramref name="item"/> replaced whatever is worn in its slot.</summary>
    public CombatStats StatsWith(ItemDefinition item)
    {
        var slot = item.Equipment?.Slot
            ?? throw new ArgumentException($"Item '{item.Id}' is not equipment.", nameof(item));

        return slot switch
        {
            EquipmentSlot.Weapon => StatsWearing(item, Armor),
            EquipmentSlot.Armor => StatsWearing(Weapon, item),
            _ => throw new ArgumentOutOfRangeException(nameof(item), slot, "Unknown equipment slot."),
        };
    }

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

    /// <returns>Null when the slot is empty.</returns>
    public ItemDefinition? EquippedIn(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Weapon => Weapon,
        EquipmentSlot.Armor => Armor,
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown equipment slot."),
    };

    /// <remarks>Only <see cref="PartyEquipment"/> calls this, after moving the copy out of the inventory.</remarks>
    internal void Wear(EquipmentSlot slot, ItemDefinition item) => FillSlot(slot, item);

    internal void TakeOff(EquipmentSlot slot) => FillSlot(slot, null);

    private void FillSlot(EquipmentSlot slot, ItemDefinition? item)
    {
        switch (slot)
        {
            case EquipmentSlot.Weapon:
                Weapon = item;
                break;
            case EquipmentSlot.Armor:
                Armor = item;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown equipment slot.");
        }
    }

    /// <exception cref="ArgumentException">A skill id isn't in the catalog.</exception>
    private static List<SkillDefinition> SkillsOf(PartyMemberDefinition definition, IReadOnlyDictionary<string, SkillDefinition> skillCatalog)
    {
        var unknownSkillId = definition.SkillIds.FirstOrDefault(id => !skillCatalog.ContainsKey(id));
        if (unknownSkillId is not null)
        {
            throw new ArgumentException($"'{definition.Id}' has unknown skill id '{unknownSkillId}'.", nameof(skillCatalog));
        }

        return [.. definition.SkillIds.Select(id => skillCatalog[id])];
    }

    private static int AttackBonusOf(ItemDefinition? item) => item?.Equipment?.AttackBonus ?? 0;

    private static int DefenseBonusOf(ItemDefinition? item) => item?.Equipment?.DefenseBonus ?? 0;

    private CombatStats StatsWearing(ItemDefinition? weapon, ItemDefinition? armor) => new(
        BaseStats.Attack + AttackBonusOf(weapon) + AttackBonusOf(armor),
        BaseStats.Defense + DefenseBonusOf(weapon) + DefenseBonusOf(armor));

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
