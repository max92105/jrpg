using System.Diagnostics.CodeAnalysis;
using Jrpg.Core.Items;
using Jrpg.Core.Progression;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Party;

/// <summary>A recruited party member's runtime state.</summary>
public sealed class PartyMember
{
    private readonly Dictionary<string, int> _lastingAmounts;

    // A new member: level 1, no experience, full HP, every lasting resource at its start (decision 168).
    private PartyMember(PartyMemberDefinition definition, ClassDefinition characterClass, IReadOnlyList<SkillDefinition> skills, StatRules rules)
    {
        Definition = definition;
        Class = characterClass;
        Skills = [.. skills];
        Rules = rules;
        Level = 1;
        RebuildStats();
        Vitals = new Vitals(Sheet.MaxHp, Sheet.MaxHp);
        _lastingAmounts = characterClass.Resources
            .Where(resource => resource.IsLasting)
            .ToDictionary(resource => resource.Id, resource => resource.StartsAt);
    }

    public string Id => Definition.Id;

    public PartyMemberDefinition Definition { get; }

    public ClassDefinition Class { get; }

    public int Level { get; private set; }

    /// <summary>Experience earned toward the next level; resets to 0 on level-up.</summary>
    public int Experience { get; private set; }

    /// <summary>
    /// The member's numbers at their level with what they wear (ADR 0026): primaries, max HP, speed (it sets readiness
    /// charges, question 213), the attack damage (the worn weapon's or the unarmed one) and the derived percents.
    /// Rebuilt on level-up and when what they wear changes.
    /// </summary>
    public StatSheet Sheet { get; private set; }

    public Vitals Vitals { get; }

    public ItemDefinition? Weapon { get; private set; }

    public ItemDefinition? Armor { get; private set; }

    public IReadOnlyList<SkillDefinition> Skills { get; }

    /// <summary>The stat conversions the sheet is built with; the battle also reads the unarmed type.</summary>
    internal StatRules Rules { get; }

    /// <summary>
    /// The amount each of the class's lasting resources carries into the next battle (ADR 0024), by resource id; it
    /// starts at the resource's starting amount, also for one a save doesn't hold.
    /// </summary>
    public IReadOnlyDictionary<string, int> LastingAmounts => _lastingAmounts;

    /// <summary>A new member joins at level 1 with 0 experience and full HP (decision 168).</summary>
    /// <param name="definition">Who joins.</param>
    /// <param name="characterClass">The member's class, which gives their stats at their level (ADR 0017).</param>
    /// <param name="skillCatalog">All skills by id; the member gets their class's, in its order (ADR 0020).</param>
    /// <param name="rules">The stat conversions, kept: max HP follows Constitution at every level (ADR 0026).</param>
    /// <exception cref="ArgumentException">A skill id isn't in the catalog.</exception>
    public static PartyMember Recruit(
        PartyMemberDefinition definition, ClassDefinition characterClass, IReadOnlyDictionary<string, SkillDefinition> skillCatalog, StatRules rules) =>
        new(definition, characterClass, SkillsOf(characterClass, skillCatalog), rules);

    /// <summary>
    /// Puts a saved member's progress back on a new recruit: level, experience, lasting amounts, and HP within that
    /// level's max. <c>GameSession</c> puts the worn items on first, so the saved HP fits a max their bonuses raise.
    /// </summary>
    /// <remarks>The progress is assumed repaired against current content by Data (ADR 0007).</remarks>
    internal void RestoreProgress(PartyMemberProgress progress)
    {
        Level = progress.Level;
        Experience = progress.Experience;
        RebuildStats();
        Vitals.ResizeMax(Sheet.MaxHp);

        // A recruit is standing at full HP: fill it to the new max, then take off what the save had lost.
        Vitals.RestoreHp(Vitals.MaxHp);
        Vitals.TakeDamage(Vitals.MaxHp - progress.Hp);
        foreach (var resource in Class.Resources.Where(resource => resource.IsLasting))
        {
            _lastingAmounts[resource.Id] = progress.LastingAmounts.GetValueOrDefault(resource.Id, resource.StartsAt);
        }
    }

    /// <summary>What a save keeps of this member.</summary>
    public PartyMemberProgress CaptureProgress() =>
        new(Id, Level, Experience, Vitals.Hp, Weapon?.Id, Armor?.Id, new Dictionary<string, int>(_lastingAmounts));

    /// <summary>The sheet with what's worn in <paramref name="slot"/> taken off, for the Equipment page.</summary>
    public StatSheet SheetWithout(EquipmentSlot slot) => SheetWearing(WornWith(slot, null));

    /// <summary>The sheet if <paramref name="item"/> replaced whatever is worn in its slot.</summary>
    /// <exception cref="ArgumentException"><paramref name="item"/> isn't equipment.</exception>
    public StatSheet SheetWith(ItemDefinition item) => SheetWearing(WornWith(SlotOf(item), item));

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

    /// <summary>Whether a rest would change anything: HP below max, or a lasting resource off its start.</summary>
    public bool HasAnythingToRest =>
        Vitals.Hp < Vitals.MaxHp || Class.Resources.Any(resource => resource.IsLasting && _lastingAmounts[resource.Id] != resource.StartsAt);

    /// <summary>
    /// A rest (decision 230): full HP, the fallen revived, and every lasting resource back at its starting amount.
    /// </summary>
    public void Rest()
    {
        if (Vitals.IsAlive)
        {
            Vitals.RestoreHp(Vitals.MaxHp);
        }
        else
        {
            Vitals.Revive(Vitals.MaxHp);
        }

        foreach (var resource in Class.Resources.Where(resource => resource.IsLasting))
        {
            _lastingAmounts[resource.Id] = resource.StartsAt;
        }
    }

    /// <summary>Keeps what a battle left of a lasting resource, within 0..its max.</summary>
    /// <exception cref="ArgumentException">The class has no lasting resource <paramref name="resourceId"/>.</exception>
    internal void KeepLastingAmount(string resourceId, int amount)
    {
        if (!_lastingAmounts.ContainsKey(resourceId))
        {
            throw new ArgumentException($"Class '{Class.Id}' has no lasting resource '{resourceId}'.", nameof(resourceId));
        }

        var max = Class.Resources.First(resource => resource.Id == resourceId).Max;
        _lastingAmounts[resourceId] = Math.Clamp(amount, 0, max);
    }

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

        // Max HP follows the sheet, keeping HP within it (decision 328).
        RebuildStats();
        Vitals.ResizeMax(Sheet.MaxHp);
    }

    private static StatBonuses BonusesOf(Worn worn) =>
        (worn.Weapon?.Equipment?.Bonuses ?? StatBonuses.None).Plus(worn.Armor?.Equipment?.Bonuses ?? StatBonuses.None);

    private static EquipmentSlot SlotOf(ItemDefinition item) =>
        item.Equipment?.Slot ?? throw new ArgumentException($"Item '{item.Id}' is not equipment.", nameof(item));

    // What would be worn with the slot holding the item (or nothing) and the other slot as now.
    private Worn WornWith(EquipmentSlot slot, ItemDefinition? item) => slot switch
    {
        EquipmentSlot.Weapon => new Worn(item, Armor),
        EquipmentSlot.Armor => new Worn(Weapon, item),
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown equipment slot."),
    };

    private StatSheet SheetWearing(Worn worn) =>
        MemberSheet.Of(ClassGrowth.StatsAt(Class, Level), BonusesOf(worn), worn.Weapon?.Equipment?.Damage, Rules);

    /// <exception cref="ArgumentException">A skill id isn't in the catalog.</exception>
    private static List<SkillDefinition> SkillsOf(ClassDefinition characterClass, IReadOnlyDictionary<string, SkillDefinition> skillCatalog)
    {
        var unknownSkillId = characterClass.SkillIds.FirstOrDefault(id => !skillCatalog.ContainsKey(id));
        if (unknownSkillId is not null)
        {
            throw new ArgumentException($"Class '{characterClass.Id}' has unknown skill id '{unknownSkillId}'.", nameof(skillCatalog));
        }

        return [.. characterClass.SkillIds.Select(id => skillCatalog[id])];
    }

    // The class's numbers at the member's level, the one formula (ADR 0017), with what they wear.
    [MemberNotNull(nameof(Sheet))]
    private void RebuildStats() => Sheet = SheetWearing(new Worn(Weapon, Armor));

    // The two slots' items, as worn or as a preview would have them.
    private readonly record struct Worn(ItemDefinition? Weapon, ItemDefinition? Armor);

    private LevelProgressStep AdvanceTowardNextLevel(int amount, int experienceToNextLevel)
    {
        var from = Experience;
        Experience += Math.Min(amount, experienceToNextLevel - from);
        var step = new LevelProgressStep(Level, from, Experience, experienceToNextLevel);

        if (step.IsLevelUp)
        {
            LevelUp();
        }

        return step;
    }

    // Vitals rise by what the max gained with the same gear on: the class's gain, Constitution's percent included.
    private void LevelUp()
    {
        var maxHpBefore = Sheet.MaxHp;
        Level++;
        Experience = 0;
        RebuildStats();
        Vitals.RaiseMaxima(Sheet.MaxHp - maxHpBefore);
    }
}
