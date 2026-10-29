using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Data.Content;
using static System.FormattableString;

namespace Jrpg.Simulations.Battle;

/// <summary>
/// The forest's fights: a level 1 leader alone against the goblin trio, then the stone beetles (decision 363),
/// attacking only, once for each class, since any class can lead (decision 168), bare and in the new game's starting
/// gear (G2). A lone level 1 leader must win the start area's fights (question 184, ADR 0017). The summary counts the
/// leader's own attacks: how many missed, how many crit, and what a landed one dealt on average.
/// </summary>
public sealed class ForestFightsSimulation(ITestOutputHelper output)
{
    private const int Battles = 1_000;
    private const int TurnLimit = 500;
    private const string GoblinTrioId = "goblin_trio";
    private const string StoneBeetlesId = "stone_beetle";

    // The member whose skills and art the leader has; only the class's numbers matter when attacking only.
    private const string StandInId = "sarxian";

    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    public static TheoryData<string> ClassIds => [.. _catalog.Classes.Keys];

    [Theory]
    [MemberData(nameof(ClassIds))]
    public void GoblinTrio_LoneLevelOneLeaderAttackingOnly_WinsEveryBattle(string classId) => AssertEveryBattleWon(GoblinTrioId, classId, Loadout.Bare);

    [Theory]
    [MemberData(nameof(ClassIds))]
    public void GoblinTrio_LoneLevelOneLeaderWithStartingGear_WinsEveryBattle(string classId) => AssertEveryBattleWon(GoblinTrioId, classId, Loadout.StartingGear);

    [Theory]
    [MemberData(nameof(ClassIds))]
    public void StoneBeetles_LoneLevelOneLeaderAttackingOnly_WinsEveryBattle(string classId) =>
        AssertEveryBattleWon(StoneBeetlesId, classId, Loadout.Bare);

    [Theory]
    [MemberData(nameof(ClassIds))]
    public void StoneBeetles_LoneLevelOneLeaderWithStartingGear_WinsEveryBattle(string classId) =>
        AssertEveryBattleWon(StoneBeetlesId, classId, Loadout.StartingGear);

    private void AssertEveryBattleWon(string encounterId, string classId, Loadout loadout)
    {
        var results = Enumerable.Range(0, Battles).Select(seed => RunBattle(encounterId, _catalog.Classes[classId], loadout, seed)).ToList();

        WriteSummary(encounterId, classId, loadout, results);
        var losses = results.Where(result => result.Phase != BattlePhase.Victory).Select(result => result.Seed).ToList();
        Assert.True(losses.Count == 0, $"A level 1 {classId} ({loadout}) against {encounterId} lost or ran out of turns on {losses.Count} of {Battles} seeds, the first {losses.FirstOrDefault()}.");
    }

    // A fresh leader per battle: battle damage persists on a member (shared Vitals), so reusing one would leak between seeds.
    private static BattleResult RunBattle(string encounterId, ClassDefinition characterClass, Loadout loadout, int seed)
    {
        var leader = PartyMember.Recruit(_catalog.PartyMembers[StandInId], characterClass, _catalog.Skills, _catalog.Rules.Stats);
        if (loadout == Loadout.StartingGear)
        {
            WearStartingGear(leader);
        }

        var party = new PartyState([leader], [leader], _catalog.Rules.Party);
        var enemies = _catalog.Encounters[encounterId].EnemyIds.Select(id => _catalog.Enemies[id]).ToList();
        var random = new SeededRandom(seed);
        var battle = BattleState.Start(party, enemies, new Inventory(), random);
        var tactics = new EnemyTactics(random);

        var turns = 0;
        var hits = HitTally.None;
        for (; turns < TurnLimit && !battle.IsOver; turns++)
        {
            var command = battle.Phase == BattlePhase.PartyTurn ? AttackFirstLivingEnemyPolicy.ChooseCommand(battle) : tactics.ChooseCommand(battle);
            hits = hits.Plus(HitTally.OfLeader(battle.Execute(command)));
        }

        return new BattleResult(seed, battle.Phase, turns, leader.Vitals.Hp, hits);
    }

    // The first starting item that fits each slot, in the bag's order (decision 332); every slot must end up worn, or
    // the geared numbers would quietly be a bare leader's.
    private static void WearStartingGear(PartyMember leader)
    {
        var bag = new Inventory();
        foreach (var entry in _catalog.NewGame.StartingItems)
        {
            bag.Add(_catalog.Items[entry.ItemId], entry.Quantity);
        }

        var equipment = new PartyEquipment(bag);
        foreach (var slot in Enum.GetValues<EquipmentSlot>())
        {
            var owned = equipment.OwnedFor(slot);
            Assert.True(owned.Count > 0, $"The starting gear has nothing that fits the {slot} slot.");
            Assert.True(equipment.TryEquip(leader, owned[0], slot), $"The starting gear's {owned[0].Base.Id} didn't go on in the {slot} slot.");
        }
    }

    private void WriteSummary(string encounterId, string classId, Loadout loadout, IReadOnlyList<BattleResult> results)
    {
        // Invariant culture, so a baseline copied from one machine compares with another's.
        var wins = results.Where(result => result.Phase == BattlePhase.Victory).ToList();
        var hpLeft = wins.Count == 0 ? "n/a" : Invariant($"{wins.Average(result => result.LeaderHpLeft):F1}");
        var hits = results.Select(result => result.Hits).Aggregate(HitTally.None, (total, tally) => total.Plus(tally));
        var gear = loadout == Loadout.StartingGear ? "in the starting gear" : "bare";
        output.WriteLine($"{encounterId}, a level 1 {classId} alone, {gear}, attack only, {Battles} seeds");
        output.WriteLine(Invariant($"  Win rate          {100.0 * wins.Count / results.Count,6:F1} %"));
        output.WriteLine(Invariant($"  Average turns     {results.Average(result => result.Turns),6:F1}"));
        output.WriteLine(Invariant($"  Attacks missed    {100.0 * hits.Misses / hits.Attacks,6:F1} % (the leader's)"));
        var crits = hits.Landed == 0 ? "n/a" : Invariant($"{100.0 * hits.Crits / hits.Landed:F1} %");
        var averageHit = hits.Landed == 0 ? "n/a" : Invariant($"{(double)hits.Damage / hits.Landed:F1}");
        output.WriteLine($"  Hits critical     {crits,8} (the leader's)");
        output.WriteLine($"  Average hit       {averageHit,6} (the leader's)");
        output.WriteLine($"  Average HP left   {hpLeft,6} (wins only)");
    }

    private enum Loadout
    {
        Bare,
        StartingGear,
    }

    private sealed record BattleResult(int Seed, BattlePhase Phase, int Turns, int LeaderHpLeft, HitTally Hits);

    /// <summary>The leader's attacks: how many missed, how many landed ones crit, and what they dealt in all.</summary>
    private sealed record HitTally(int Attacks, int Misses, int Crits, int Damage)
    {
        public static HitTally None { get; } = new(0, 0, 0, 0);

        public int Landed => Attacks - Misses;

        // The leader is the only party member, so a hit on an enemy is theirs.
        public static HitTally OfLeader(IReadOnlyList<BattleEvent> events)
        {
            var landed = events.OfType<EffectApplied>().Where(effect => effect.Kind == EffectKind.Damage && effect.Target.Side == BattleSide.Enemies).ToList();
            var misses = events.OfType<HitMissed>().Count(miss => miss.Target.Side == BattleSide.Enemies);
            return new HitTally(landed.Count + misses, misses, landed.Count(effect => effect.IsCritical), landed.Sum(effect => effect.Amount));
        }

        public HitTally Plus(HitTally other) => new(Attacks + other.Attacks, Misses + other.Misses, Crits + other.Crits, Damage + other.Damage);
    }
}
