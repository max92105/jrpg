namespace Jrpg.Core.Stats;

/// <summary>
/// The primaries-to-derived conversions, in one place (ADR 0026). Each is a percent per point with no level divisor,
/// applied to a base that grows, so a point keeps its worth at every level; each rounds down once, at the end. Derived
/// percents are in permille (tenths of a percent), so a small bonus still moves the number.
/// </summary>
public static class StatFormula
{
    /// <summary>A crit's damage before Power: half again a normal hit (decision 300).</summary>
    public const int BaseCritDamagePercent = 150;

    /// <summary>The most dodge anyone has, so no one is out of reach (decision 295).</summary>
    public const int DodgeCapPercent = 50;

    private const int PermillePerPercent = 10;

    // Rates are percents per 100 points, so a percent per point is rate / 100 and the whole factor is over 100 x 100.
    private const long PercentPerPointScale = 100L * 100;

    /// <summary>
    /// The class's max HP at the member's level raised by Constitution:
    /// <c>classMaxHp × (100 + constitution × rate ÷ 100) ÷ 100</c>. A percent of a base that grows, so Constitution,
    /// from the class or from gear, keeps its worth as the member levels (ADR 0026): 220 HP and 14 Constitution at 100
    /// per 100 points give 250.
    /// </summary>
    public static int MaxHp(int classMaxHp, int constitution, StatRules rules) =>
        (int)(classMaxHp * (PercentPerPointScale + ((long)constitution * rules.HpPercentPer100Constitution)) / PercentPerPointScale);

    /// <summary>
    /// The rules' base accuracy plus the class's points, in permille: the hit chance against 0 dodge (decision 297).
    /// </summary>
    public static int AccuracyPermille(int classAccuracy, StatRules rules) =>
        (rules.BaseAccuracy + classAccuracy) * PermillePerPercent;

    /// <summary>The base crit chance plus Agility's percent, in permille: 20 Agility at 25 per 100 adds 5 %.</summary>
    public static int CritChancePermille(int agility, StatRules rules) =>
        (rules.BaseCritChance * PermillePerPercent) + PerPointPermille(agility, rules.CritChancePer100Agility);

    /// <summary>A crit's damage, 150 % plus Power's percent, in permille (decision 300).</summary>
    public static int CritDamagePermille(int power, StatRules rules) =>
        (BaseCritDamagePercent * PermillePerPercent) + PerPointPermille(power, rules.CritDamagePercentPer100Power);

    /// <summary>
    /// The base dodge plus Agility's percent plus gear's, in permille, never above the cap (decision 295): capped once,
    /// after gear, so a cursed item lowers a dodge that was over the cap from where it was, not from the cap.
    /// </summary>
    public static int DodgePermille(int agility, int gearPercent, StatRules rules) => Math.Min(
        ((rules.BaseDodge + gearPercent) * PermillePerPercent) + PerPointPermille(agility, rules.DodgePer100Agility),
        DodgeCapPercent * PermillePerPercent);

    /// <summary>
    /// The chance to shrug off a stun or a taunt, in permille: the rules' base, which gear raises (decision 302).
    /// </summary>
    public static int CrowdControlResistancePermille(StatRules rules) =>
        rules.BaseCrowdControlResistance * PermillePerPercent;

    /// <summary>
    /// What every raw damage and healing the member deals is multiplied by, in permille: 100 % plus Power's percent
    /// (decision 296: Power is the universal damage stat).
    /// </summary>
    public static int DamagePermille(int power, StatRules rules) =>
        (100 * PermillePerPercent) + PerPointPermille(power, rules.DamagePercentPer100Power);

    /// <summary>
    /// Constitution's percent on the three physical resistances, Will's on the five magical ones, plus gear's, in
    /// permille and within the floor and cap once gear is added (ADR 0027): the two primaries that keep a member
    /// standing, each against its kind of hit.
    /// </summary>
    public static Resistances ResistancesFrom(PrimaryStats primaries, Resistances gear, StatRules rules) => Resistances.ByGroup(
        PerPointPermille(primaries.Constitution, rules.PhysicalResistancePer100Constitution),
        PerPointPermille(primaries.Will, rules.MagicalResistancePer100Will)).Plus(gear).Clamped();

    /// <summary>
    /// What a status the member grants has its power multiplied by, in permille: 100 % plus Intelligence's percent
    /// (decision 296: Intelligence shapes what a skill leaves behind).
    /// </summary>
    public static int StatusPermille(int intelligence, StatRules rules) =>
        (100 * PermillePerPercent) + PerPointPermille(intelligence, rules.StatusPercentPer100Intelligence);

    /// <summary>
    /// The turns added to a status the member grants: one per breakpoint of Intelligence, rounded down, so it grows in
    /// steps a player can aim for. Never applied to Stun (decision 317).
    /// </summary>
    public static int ExtraStatusTurns(int intelligence, StatRules rules) =>
        intelligence / rules.IntelligencePerExtraStatusTurn;

    /// <summary>
    /// A raw damage or healing as the member deals it, raised by their <see cref="DamagePermille"/> (decision 296): in
    /// a <c>long</c>, rounded down once (ADR 0027), so a 24 damage weapon at 115 % deals 27.
    /// </summary>
    public static int Scaled(int raw, int damagePermille) => (int)((long)raw * damagePermille / PermilleOf(100));

    /// <summary>
    /// A member's basic attack as they deal it (decision 337): the sheet's attack damage raised by its damage percent,
    /// the one number every page shows for it.
    /// </summary>
    public static int WeaponDamage(StatSheet sheet) => Scaled(sheet.AttackDamage, sheet.DamagePermille);

    /// <summary>A whole percent as permille, the unit derived percents are held in (ADR 0026).</summary>
    public static int PermilleOf(int percent) => percent * PermillePerPercent;

    /// <summary>
    /// A permille as the whole percent a screen shows, rounded down (ADR 0026), toward the lower number: 97 is 9 %, and
    /// a weakness of -25.5 % (-255) is -26 %.
    /// </summary>
    public static int WholePercentOf(int permille) => (int)Math.Floor(permille / (double)PermillePerPercent);

    // Points at a rate per 100 points, in permille, rounded down: 3 Agility at 25 per 100 is 7 (0.75 %, shown 0 %).
    private static int PerPointPermille(int points, int ratePer100) =>
        (int)((long)points * ratePer100 * PermillePerPercent / 100);
}
