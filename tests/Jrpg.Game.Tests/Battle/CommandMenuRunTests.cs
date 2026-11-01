using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Input;

namespace Jrpg.Game.Tests.Battle;

// Run is greyed while a boss is in the battle (decision 407), as Core's CanFlee says.
public sealed class CommandMenuRunTests
{
    private readonly Inventory _inventory = new();

    [Fact]
    public void IsEnabled_BossBattle_RunIsGreyed()
    {
        var menu = MenuFor(BossBattle());

        Assert.False(menu.IsEnabled(CommandEntry.Run));
    }

    [Fact]
    public void IsEnabled_NoBoss_RunIsOffered()
    {
        var battle = BattleScenarios.MemberAgainstGoblinTrio("zanyr", _inventory, new SeededRandom(BattleScenarios.SarxianFirstSeed));

        var menu = MenuFor(battle);

        Assert.True(menu.IsEnabled(CommandEntry.Run));
    }

    [Fact]
    public void Update_DownFromDefendInABossBattle_NeverSelectsRun()
    {
        var menu = MenuFor(BossBattle());
        for (var presses = 0; presses < CommandMenu.Entries.Length && menu.Selected != CommandEntry.Defend; presses++)
        {
            menu.Update(InputFrames.Pressing(GameButton.Down));
        }

        Assert.Equal(CommandEntry.Defend, menu.Selected);

        menu.Update(InputFrames.Pressing(GameButton.Down));

        Assert.NotEqual(CommandEntry.Run, menu.Selected);
    }

    private CommandMenu MenuFor(BattleState battle)
    {
        var menu = new CommandMenu(_inventory, BattleScenarios.Catalog.Items);
        menu.Start(battle);
        return menu;
    }

    // Three enemies, as the seed named for the trio expects, so the member acts first.
    private BattleState BossBattle()
    {
        var member = BattleScenarios.Recruit("zanyr");
        var party = new PartyState([member], [member], BattleScenarios.Catalog.Rules.Party);
        var enemies = new[] { BattleScenarios.Goblin, BattleScenarios.Goblin, BattleScenarios.Goblin with { IsBoss = true } };
        return BattleState.Start(party, enemies, _inventory, new SeededRandom(BattleScenarios.SarxianFirstSeed));
    }
}
