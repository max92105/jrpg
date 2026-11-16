using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Data.Content;
using Jrpg.Editor.Skills;

namespace Jrpg.Editor.Tests.Skills;

/// <summary>
/// The skill, effect and status tests' own records, so no repository number is pinned: a fire_1 tier, a poison_1 status
/// and a bolt skill spending 2 mana, its effect naming both, over the test world's catalog.
/// </summary>
internal static class SkillFixtures
{
    /// <summary>The test world's catalog, whose classes and enemies a test picks by id to build its own.</summary>
    public static ContentCatalog TestWorld { get; } =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    /// <summary>A Skills page with nothing in its boxes' folders and no strip record.</summary>
    public static SkillChoices NoChoices { get; } = new([], [], [], new Dictionary<string, EffectStripDefinition>(), [], []);

    public static TieredEffect Fire1 { get; } = new("fire_1", EffectKind.Damage, 3) { DamageType = DamageType.Fire };

    public static StatusDefinition Poison1 { get; } = new()
    {
        Id = "poison_1",
        Name = "Poison",
        Description = "Loses HP each turn.",
        IsHarmful = true,
        Parts = [new StatusPart(StatusPartKind.DamageEachTurn, 4)],
    };

    public static SkillDefinition Bolt { get; } = new()
    {
        Id = "bolt",
        Name = "Bolt",
        Description = "A bolt of fire that poisons.",
        IconFile = "Bolt.png",
        SoundFile = "Bolt.wav",
        EffectFile = "BoltStrip.png",
        ResourceChanges = [new ResourceChange("mana", -2)],
        Requirements = [new ResourceRequirement("mana", 2)],
        Effect = new Effect(EffectKind.Damage, 0, Targeting.SingleEnemy, DamageType: DamageType.Fire)
        {
            Tier = Fire1,
            Statuses = [new StatusGrant(Poison1, 2) { Chance = 50 }],
        },
    };

    /// <summary>The test world's catalog holding only the given skills, tiers and statuses.</summary>
    public static ContentCatalog CatalogWith(
        IReadOnlyList<SkillDefinition> skills,
        IReadOnlyList<TieredEffect> tiers,
        IReadOnlyList<StatusDefinition> statuses) =>
        TestWorld with
        {
            Skills = skills.ToDictionary(skill => skill.Id, StringComparer.Ordinal),
            TieredEffects = tiers.ToDictionary(tier => tier.Id, StringComparer.Ordinal),
            Statuses = statuses.ToDictionary(status => status.Id, StringComparer.Ordinal),
        };

    /// <summary>
    /// <see cref="CatalogWith"/> with poison_1 and no class or enemy: the test world's name skills these catalogs lack,
    /// which Data's check would report; a test of who uses a skill adds its own.
    /// </summary>
    public static ContentCatalog CatalogWithoutUsers(IReadOnlyList<SkillDefinition> skills, IReadOnlyList<TieredEffect> tiers) =>
        CatalogWith(skills, tiers, [Poison1]) with
        {
            Classes = new Dictionary<string, ClassDefinition>(),
            Enemies = new Dictionary<string, EnemyDefinition>(),
        };
}
