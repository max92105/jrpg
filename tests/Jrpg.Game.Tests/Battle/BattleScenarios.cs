using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Data.Content;

namespace Jrpg.Game.Tests.Battle;

/// <summary>Battles on the repository's content, for tests of the battle screen's pure parts.</summary>
internal static class BattleScenarios
{
    /// <summary>The seed that shuffles the three goblins ahead of Sarxian.</summary>
    public const int GoblinsFirstSeed = 1;

    /// <summary>The seed that keeps the listed order: Sarxian, then goblins A, B and C.</summary>
    public const int SarxianFirstSeed = 4;

    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    /// <param name="random">Shuffles the turn order; tests that run enemy turns pass the same source to the tactics.</param>
    public static BattleState SarxianAgainstGoblinTrio(IRandom random)
    {
        var sarxian = PartyMember.Recruit(_catalog.PartyMembers["sarxian"], _catalog.Skills);
        var party = new PartyState([sarxian], [sarxian], _catalog.Rules.Party);
        var goblin = _catalog.Enemies["goblin"];
        return BattleState.Start(party, [goblin, goblin, goblin], new Inventory(), random);
    }
}
