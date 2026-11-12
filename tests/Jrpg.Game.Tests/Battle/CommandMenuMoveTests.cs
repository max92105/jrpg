using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Input;

namespace Jrpg.Game.Tests.Battle;

// The Move entry (ADR 0023): trading places with the one ally beside the actor.
public sealed class CommandMenuMoveTests
{
    private readonly Inventory _inventory = new();

    [Fact]
    public void Update_ConfirmMove_TargetsOnlyNeighbors()
    {
        var (menu, battle) = TwoMembersMenu();
        CommandMenuDriver.SelectEntry(menu, CommandEntry.Move);

        menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.Equal(battle.TargetChoiceForMove().Candidates, menu.Targets?.Candidates);
    }

    [Fact]
    public void Update_ConfirmMoveTarget_ProducesMoveCommand()
    {
        var (menu, battle) = TwoMembersMenu();
        CommandMenuDriver.SelectEntry(menu, CommandEntry.Move);
        menu.Update(InputFrames.Pressing(GameButton.Confirm));

        var command = menu.Update(InputFrames.Pressing(GameButton.Confirm));

        var neighbor = Assert.Single(battle.TargetChoiceForMove().Candidates);
        Assert.Equal(new MoveCommand(neighbor), command);
    }

    [Fact]
    public void Update_CancelOnMoveTargets_ReturnsToCommands()
    {
        var (menu, _) = TwoMembersMenu();
        CommandMenuDriver.SelectEntry(menu, CommandEntry.Move);
        menu.Update(InputFrames.Pressing(GameButton.Confirm));

        menu.Update(InputFrames.Pressing(GameButton.Cancel));

        Assert.Null(menu.Targets);
        Assert.Equal(CommandEntry.Move, menu.Selected);
    }

    // Zanyr and Ragnir against the goblins, who defend until one of the two acts; each is the other's one neighbor.
    private (CommandMenu Menu, BattleState Battle) TwoMembersMenu()
    {
        var (zanyr, ragnir) = (BattleScenarios.Recruit("zanyr"), BattleScenarios.Recruit("ragnir"));
        var party = new PartyState([zanyr, ragnir], [zanyr, ragnir], BattleScenarios.Catalog.Rules.Party);
        var goblin = BattleScenarios.Catalog.Enemies["goblin"];
        var battle = BattleState.Start(party, [goblin, goblin, goblin], _inventory, new SeededRandom(BattleScenarios.SarxianFirstSeed));
        while (battle.CurrentActor.Side == BattleSide.Enemies)
        {
            battle.Execute(new DefendCommand());
        }

        var menu = new CommandMenu(_inventory, BattleScenarios.Catalog.Items);
        menu.Start(battle);
        return (menu, battle);
    }
}
