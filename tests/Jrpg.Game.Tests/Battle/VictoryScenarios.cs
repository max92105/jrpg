using Jrpg.Core.Battle;
using Jrpg.Core.Loot;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Session;

namespace Jrpg.Game.Tests.Battle;

/// <summary>A won battle's spoils for the victory screen's tests, on Sarxian from the repository's content.</summary>
internal static class VictoryScenarios
{
    public static PartyMember Sarxian() => BattleScenarios.Recruit("sarxian");

    /// <summary>Sarxian's 60 experience filling half of an empty 120-point level: no level up.</summary>
    public static ExperienceGain HalfALevel() =>
        new(Sarxian(), 60, [new LevelProgressStep(1, 0, 60, 120)], IsAtMaxLevel: false);

    /// <summary>The one gain as the battle's whole experience, with nothing dropped and no gold.</summary>
    public static BattleSpoils Spoils(ExperienceGain gain) => Spoils(new VictoryRewards(gain.Amount, [gain]));

    /// <summary>The rewards with nothing dropped and no gold.</summary>
    public static BattleSpoils Spoils(VictoryRewards rewards) => new(rewards, Drops.None, Gold: 0);
}
