using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Core.Stats;
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

    /// <summary>
    /// The repository's rules with every hit certain and no crit: accuracy 100 against the scenario goblin's 0 dodge,
    /// no dodge and no crit chance on a member, so a seeded scenario draws nothing for its hits and keeps its sequence
    /// (ADR 0026).
    /// </summary>
    public static StatRules CertainRules { get; } = Catalog.Rules.Stats with
    {
        BaseAccuracy = 100,
        BaseDodge = 0,
        DodgePer100Agility = 0,
        BaseCritChance = 0,
        CritChancePer100Agility = 0,
    };

    /// <summary>The repository's goblin, hitting for certain, never dodging and never critting.</summary>
    public static EnemyDefinition Goblin { get; } = Catalog.Enemies["goblin"] with { Accuracy = 100, Dodge = 0, CritChance = 0 };

    /// <summary>
    /// A member of the repository's content as they join: level 1, with their class's numbers under
    /// <see cref="CertainRules"/>.
    /// </summary>
    public static PartyMember Recruit(string memberId)
    {
        var definition = Catalog.PartyMembers[memberId];
        return PartyMember.Recruit(definition, Catalog.ClassOf(definition), Catalog.Skills, CertainRules);
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
        return BattleState.Start(party, [Goblin, Goblin, Goblin], inventory, random);
    }
}
