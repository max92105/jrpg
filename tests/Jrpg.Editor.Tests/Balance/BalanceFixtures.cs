using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;

namespace Jrpg.Editor.Tests.Balance;

/// <summary>
/// The balance table tests' own classes, skills and enemies, so no repository number is pinned: a mage of Power 12 (+3
/// a level) and a cleric of Power 10 (+2), the mage first so a list in id order is sorted, not read in the dictionary's
/// order, and the imp before the bat for the same reason; skills of every row shape, one learned at level 5; rules
/// where each Power point adds 1 % to a flat power.
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

    /// <summary>
    /// Power 20, 80 HP and a fire attack of 9; its tactics are the attack, Fireball, Curse and Mend, and below half its
    /// HP its phase names Flurry, then Fireball once more.
    /// </summary>
    public static EnemyDefinition Imp { get; } = Enemy("imp", "Imp") with
    {
        MaxHp = 80,
        Damage = 9,
        DamageType = DamageType.Fire,
        Power = 20,
        Tactics = [Tactic(skillId: null), Tactic(Fireball.Id), Tactic(Curse.Id), Tactic(Mend.Id)],
        Phases = [new EnemyPhase { BelowHpPercent = 50, Tactics = [Tactic(Flurry.Id), Tactic(Fireball.Id)] }],
    };

    /// <summary>40 HP, a blunt attack of 5, Power 0, and Smite, the flat skill.</summary>
    public static EnemyDefinition Bat { get; } = Enemy("bat", "Bat") with { Tactics = [Tactic(Smite.Id)] };

    /// <summary>
    /// The test world with the two classes, their six skills and the two enemies only, the imp first; rules where each
    /// Constitution point adds 1 % to a class's max HP.
    /// </summary>
    public static ContentCatalog World { get; } = WorldOf(
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));

    private static ContentCatalog WorldOf(ContentCatalog loaded) => loaded with
    {
        Classes = new Dictionary<string, ClassDefinition> { [Mage.Id] = Mage, [Cleric.Id] = Cleric },
        Skills = new[] { Fireball, Flurry, Nova, Curse, Mend, Smite }.ToDictionary(skill => skill.Id),
        Enemies = new Dictionary<string, EnemyDefinition> { [Imp.Id] = Imp, [Bat.Id] = Bat },
        Rules = loaded.Rules with
        {
            Stats = loaded.Rules.Stats with { DamagePercentPer100Power = 100, HpPercentPer100Constitution = 100 },
        },
    };

    private static EnemyAction Tactic(string? skillId) => new() { SkillId = skillId, Weight = 1 };

    // Only HP, power and the attack matter to the table; the rest is any number Data takes.
    private static EnemyDefinition Enemy(string id, string name) => new()
    {
        Id = id,
        Name = name,
        PortraitFile = "",
        BattleSpriteFile = "",
        AttackEffectFile = "",
        IsSpriteFlipped = false,
        MaxHp = 40,
        Damage = 5,
        DamageType = DamageType.Blunt,
        Accuracy = 90,
        Dodge = 0,
        CritChance = 0,
        CrowdControlResistance = 0,
        ExperienceReward = 1,
    };

    private static Effect Tiered(TieredEffect tier, int hits) =>
        new(EffectKind.Damage, 0, Targeting.SingleEnemy, Hits: hits, DamageType: tier.DamageType) { Tier = tier };

    private static SkillDefinition Skill(string id, string name, Effect effect) =>
        new() { Id = id, Name = name, Description = "", IconFile = "", Effect = effect };

    // Every other stat the same for both classes: max HP 100 (+10 a level), raised by Constitution 5 (+1), beside the
    // Power that the table's amounts read.
    private static ClassDefinition Class(string id, string name, (int Base, int Growth) power) => new(
        id,
        name,
        "",
        new ClassStats(MaxHp: 100, Speed: 10) { Primaries = new PrimaryStats(power.Base, 5, 5, 5, 5) },
        new ClassStats(MaxHp: 10, Speed: 1) { Primaries = new PrimaryStats(power.Growth, 1, 1, 1, 1) });
}
