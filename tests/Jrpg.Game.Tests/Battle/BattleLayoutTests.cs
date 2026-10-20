using Jrpg.Core.Battle;
using Jrpg.Core.Party;
using Jrpg.Data.Content;
using Jrpg.Game.Battle;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Battle;

public sealed class BattleLayoutTests
{
    [Fact]
    public void EnemySpot_SecondSlot_IsOneRowUpAndOneShiftRight()
    {
        var first = BattleLayout.EnemySpot(0);

        var second = BattleLayout.EnemySpot(1);

        Assert.Equal(new Rectangle(first.X + 50, first.Y - 136, 96, 96), second);
    }

    [Fact]
    public void PartySpot_Leader_IsTopRowFurthestLeft()
    {
        var spots = Enumerable.Range(0, 3).Select(index => BattleLayout.PartySpot(index, 3)).ToList();

        Assert.Equal(spots.Min(spot => spot.Y), spots[0].Y);
        Assert.Equal(spots.Min(spot => spot.X), spots[0].X);
    }

    [Fact]
    public void SpotsOf_TwoMembersThenThreeEnemies_GivesPartySpotsThenEncounterSlots()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));
        var goblin = catalog.Enemies["goblin"];
        Combatant[] combatants =
        [
            Combatant.FromPartyMember(PartyMember.Recruit(catalog.PartyMembers["sarxian"], catalog.Skills)),
            Combatant.FromPartyMember(PartyMember.Recruit(catalog.PartyMembers["jazog"], catalog.Skills)),
            Combatant.FromEnemy(goblin),
            Combatant.FromEnemy(goblin),
            Combatant.FromEnemy(goblin),
        ];

        var spots = BattleLayout.SpotsOf(combatants);

        Assert.Equal(
            [BattleLayout.PartySpot(0, 2), BattleLayout.PartySpot(1, 2), new Rectangle(396, 864, 96, 96), BattleLayout.EnemySpot(1), BattleLayout.EnemySpot(2)],
            spots);
    }

    [Fact]
    public void PartySpot_LastMember_IsBottomRow()
    {
        var last = BattleLayout.PartySpot(2, 3);

        Assert.Equal(new Rectangle(1424, 1080 - 120 - 96, 96, 96), last);
    }
}
