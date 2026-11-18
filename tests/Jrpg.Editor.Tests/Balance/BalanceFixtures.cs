using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;

namespace Jrpg.Editor.Tests.Balance;

/// <summary>
/// The balance table tests' own classes and skills, so no repository number is pinned: a mage of Power 12 (+3 a level)
/// and a cleric of Power 10 (+2), the mage first so a list in id order is sorted, not read in the dictionary's order;
/// skills of every row shape, one learned at level 5; rules where each Power point adds 1 % to a flat power.
/// </summary>
internal static class BalanceFixtures
{
    public static TieredEffect Fire1 { get; } = new("fire_1", EffectKind.Damage, 2) { DamageType = DamageType.Fire };

    public static TieredEffect Heal1 { get; } = new("heal_1", EffectKind.RestoreHp, 3);

    public static SkillDefinition Fireball { get; } = Skill("fireball", "Fireball", Tiered(Fire1, hits: 1));

    public static SkillDefinition Flurry { get; } = Skill("flurry", "Flurry", Tiered(Fire1, hits: 4));

    /// <summary>Learned at level 5 by the mage.</summary>
    public static SkillDefinition Nova { get; } = Skill("nova", "Nova", Tiered(Fire1, hits: 1));

    public static SkillDefinition Curse { get; } =
        Skill("curse", "Curse", new Effect(EffectKind.GrantStatus, 0, Targeting.SingleEnemy));

    public static SkillDefinition Mend { get; } =
        Skill("mend", "Mend", new Effect(EffectKind.RestoreHp, 0, Targeting.SingleAlly) { Tier = Heal1 });

    public static SkillDefinition Smite { get; } =
        Skill("smite", "Smite", new Effect(EffectKind.Damage, 15, Targeting.SingleEnemy, DamageType: DamageType.Light));

    /// <summary>Fireball, Curse, Flurry, then Nova at level 5.</summary>
    public static ClassDefinition Mage { get; } =
        Class("mage", "Mage", power: (12, 3)) with
        {
            Skills = [new(Fireball.Id), new(Curse.Id), new(Flurry.Id), new(Nova.Id) { Level = 5 }],
        };

    /// <summary>Mend, then Smite.</summary>
    public static ClassDefinition Cleric { get; } =
        Class("cleric", "Cleric", power: (10, 2)) with { Skills = [new(Mend.Id), new(Smite.Id)] };

    /// <summary>The test world with the two classes and their six skills only.</summary>
    public static ContentCatalog World { get; } = WorldOf(
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));

    private static ContentCatalog WorldOf(ContentCatalog loaded) => loaded with
    {
        Classes = new Dictionary<string, ClassDefinition> { [Mage.Id] = Mage, [Cleric.Id] = Cleric },
        Skills = new[] { Fireball, Flurry, Nova, Curse, Mend, Smite }.ToDictionary(skill => skill.Id),
        Rules = loaded.Rules with { Stats = loaded.Rules.Stats with { DamagePercentPer100Power = 100 } },
    };

    private static Effect Tiered(TieredEffect tier, int hits) =>
        new(EffectKind.Damage, 0, Targeting.SingleEnemy, Hits: hits, DamageType: tier.DamageType) { Tier = tier };

    private static SkillDefinition Skill(string id, string name, Effect effect) =>
        new() { Id = id, Name = name, Description = "", IconFile = "", Effect = effect };

    // Every other stat the same for both classes; only Power matters to the table's amounts.
    private static ClassDefinition Class(string id, string name, (int Base, int Growth) power) => new(
        id,
        name,
        "",
        new ClassStats(MaxHp: 100, Speed: 10) { Primaries = new PrimaryStats(power.Base, 5, 5, 5, 5) },
        new ClassStats(MaxHp: 10, Speed: 1) { Primaries = new PrimaryStats(power.Growth, 1, 1, 1, 1) });
}
