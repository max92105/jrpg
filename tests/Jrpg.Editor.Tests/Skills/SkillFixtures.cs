using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Data.Content;

namespace Jrpg.Editor.Tests.Skills;

/// <summary>
/// The skill, effect and status tests' own records, so no repository number is pinned: a fire_1 tier, a poison_1 status
/// and a bolt skill spending 2 mana, its effect naming both, over the test world's catalog.
/// </summary>
internal static class SkillFixtures
{
    private static readonly ContentCatalog _testWorld =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

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
        _testWorld with
        {
            Skills = skills.ToDictionary(skill => skill.Id, StringComparer.Ordinal),
            TieredEffects = tiers.ToDictionary(tier => tier.Id, StringComparer.Ordinal),
            Statuses = statuses.ToDictionary(status => status.Id, StringComparer.Ordinal),
        };
}
