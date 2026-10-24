using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

public sealed class BattlerArtTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void Of_Enemy_UsesEnemySpriteAndFlip()
    {
        var goblin = _catalog.Enemies["goblin"] with { IsSpriteFlipped = true };

        var art = BattlerArt.Of(Combatant.FromEnemy(goblin));

        Assert.Equal(new BattlerArt(goblin.BattleSpriteFile, true, goblin.PortraitFile, goblin.AttackEffectFile), art);
    }

    [Fact]
    public void Of_PartyMember_UsesMemberSprite()
    {
        var sarxian = BattleScenarios.Recruit("sarxian");

        var art = BattlerArt.Of(Combatant.FromPartyMember(sarxian));

        Assert.Equal(new BattlerArt("Sarxian.png", false, "Sarxian.png", "AttackPlayer.png"), art);
    }
}
