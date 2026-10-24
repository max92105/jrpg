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

    /// <summary>The repository's content, as loaded.</summary>
    public static ContentCatalog Catalog { get; } = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    /// <summary>A member of the repository's content as they join: level 1, with their class's numbers.</summary>
    public static PartyMember Recruit(string memberId)
    {
        var definition = Catalog.PartyMembers[memberId];
        return PartyMember.Recruit(definition, Catalog.ClassOf(definition), Catalog.Skills);
    }

    /// <param name="random">Shuffles the turn order; tests that run enemy turns pass the same source to the tactics.</param>
    public static BattleState SarxianAgainstGoblinTrio(IRandom random) => MemberAgainstGoblinTrio("sarxian", new Inventory(), random);

    /// <param name="memberId">The party member who fights alone, listed first.</param>
    /// <param name="inventory">The party's items.</param>
    /// <param name="random">Shuffles the turn order; tests that run enemy turns pass the same source to the tactics.</param>
    public static BattleState MemberAgainstGoblinTrio(string memberId, Inventory inventory, IRandom random)
    {
        var member = Recruit(memberId);
        var party = new PartyState([member], [member], Catalog.Rules.Party);
        var goblin = Catalog.Enemies["goblin"];
        return BattleState.Start(party, [goblin, goblin, goblin], inventory, random);
    }
}
