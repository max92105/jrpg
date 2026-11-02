using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

// Moves, what Confirm opens or submits, and Cancel back a step; a refused choice is silent (decision 421). Omer the
// mage knows Meteor, which he can't afford at the battle's start.
public sealed class CommandMenuSoundTests
{
    private readonly Inventory _inventory = new();
    private readonly BattleState _battle;
    private readonly CommandMenu _menu;

    public CommandMenuSoundTests()
    {
        _battle = BattleScenarios.MemberAgainstGoblinTrio("omer", _inventory, new SeededRandom(BattleScenarios.SarxianFirstSeed));
        _menu = new CommandMenu(_inventory, BattleScenarios.Catalog.Items);
        _menu.Start(_battle);
    }

    [Fact]
    public void Update_Down_AsksTheMoveSound()
    {
        Press(GameButton.Down);

        Assert.Equal([UiSoundFiles.Effects.MenuMove], _menu.Sounds);
    }

    [Fact]
    public void Update_NothingPressedAfterAMove_AsksNothing()
    {
        Press(GameButton.Down);

        _menu.Update(default);

        Assert.Empty(_menu.Sounds);
    }

    [Fact]
    public void Update_CancelOnTheList_AsksNothing()
    {
        Press(GameButton.Cancel);

        Assert.Empty(_menu.Sounds);
    }

    [Fact]
    public void Update_ConfirmSkills_AsksTheConfirmSound()
    {
        Select(CommandEntry.Skills);

        Press(GameButton.Confirm);

        Assert.NotNull(_menu.Skills);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], _menu.Sounds);
    }

    [Fact]
    public void Update_DownInTheSkillList_AsksTheMoveSound()
    {
        OpenSkills();

        Press(GameButton.Down);

        Assert.Equal([UiSoundFiles.Effects.MenuMove], _menu.Sounds);
    }

    [Fact]
    public void Update_CancelInTheSkillList_AsksTheCancelSound()
    {
        OpenSkills();

        Press(GameButton.Cancel);

        Assert.Null(_menu.Skills);
        Assert.Equal([UiSoundFiles.Effects.MenuCancel], _menu.Sounds);
    }

    [Fact]
    public void Update_ConfirmOnAGreyedSkill_AsksNothing()
    {
        OpenSkills();
        var skills = _menu.Skills!;
        SelectSkill("meteor");

        Press(GameButton.Confirm);

        Assert.True(skills.IsSayingCannotAfford);
        Assert.Empty(_menu.Sounds);
    }

    [Fact]
    public void Update_ConfirmOnAUsableSkill_AsksTheConfirmSound()
    {
        OpenSkills();
        SelectSkill("fire_blast");

        Press(GameButton.Confirm);

        Assert.NotNull(_menu.Targets);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], _menu.Sounds);
    }

    [Fact]
    public void Update_DownInTheItemList_AsksTheMoveSound()
    {
        OpenItems();

        Press(GameButton.Down);

        Assert.Equal([UiSoundFiles.Effects.MenuMove], _menu.Sounds);
    }

    [Fact]
    public void Update_CancelInTheItemList_AsksTheCancelSound()
    {
        OpenItems();

        Press(GameButton.Cancel);

        Assert.Null(_menu.Items);
        Assert.Equal([UiSoundFiles.Effects.MenuCancel], _menu.Sounds);
    }

    [Fact]
    public void Update_ConfirmOnAnItem_AsksTheConfirmSound()
    {
        OpenItems();

        Press(GameButton.Confirm);

        Assert.NotNull(_menu.Targets);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], _menu.Sounds);
    }

    // The revive has nobody to raise, so it stays greyed.
    [Fact]
    public void Update_ConfirmOnAGreyedItem_AsksNothing()
    {
        OpenItems();
        var items = _menu.Items!;
        for (var presses = 0; presses < items.Items.Count && items.Selected.Id != "revive_feather"; presses++)
        {
            Press(GameButton.Down);
        }

        Assert.Equal("revive_feather", items.Selected.Id);

        Press(GameButton.Confirm);

        Assert.Null(_menu.Targets);
        Assert.Empty(_menu.Sounds);
    }

    [Fact]
    public void Update_DownOnTheTargets_AsksTheMoveSound()
    {
        Press(GameButton.Confirm);

        Press(GameButton.Down);

        Assert.Equal([UiSoundFiles.Effects.MenuMove], _menu.Sounds);
    }

    // One candidate, whoever acts first: a press in a direction that changes nothing stays silent.
    [Fact]
    public void Update_DownOnTheOnlyTarget_AsksNothing()
    {
        var member = BattleScenarios.Recruit("omer");
        var party = new PartyState([member], [member], BattleScenarios.Catalog.Rules.Party);
        var menu = new CommandMenu(_inventory, BattleScenarios.Catalog.Items);
        menu.Start(BattleState.Start(party, [BattleScenarios.Goblin], _inventory, new SeededRandom(1)));
        menu.Update(InputFrames.Pressing(GameButton.Confirm));
        Assert.Single(menu.Targets!.Candidates);

        menu.Update(InputFrames.Pressing(GameButton.Down));

        Assert.Empty(menu.Sounds);
    }

    [Fact]
    public void Update_CancelOnTheTargets_AsksTheCancelSound()
    {
        Press(GameButton.Confirm);

        Press(GameButton.Cancel);

        Assert.Null(_menu.Targets);
        Assert.Equal([UiSoundFiles.Effects.MenuCancel], _menu.Sounds);
    }

    [Fact]
    public void Update_ConfirmOnATarget_AsksTheConfirmSound()
    {
        Press(GameButton.Confirm);

        var command = _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<AttackCommand>(command);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], _menu.Sounds);
    }

    [Fact]
    public void Update_ConfirmDefend_AsksTheConfirmSound()
    {
        Select(CommandEntry.Defend);

        var command = _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<DefendCommand>(command);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], _menu.Sounds);
    }

    private void Press(GameButton button) => _menu.Update(InputFrames.Pressing(button));

    private void SelectSkill(string skillId)
    {
        var skills = _menu.Skills!;
        for (var presses = 0; presses < skills.Skills.Count && skills.Selected.Id != skillId; presses++)
        {
            Press(GameButton.Down);
        }

        Assert.Equal(skillId, skills.Selected.Id);
    }

    private void OpenSkills()
    {
        Select(CommandEntry.Skills);
        Press(GameButton.Confirm);
        Assert.NotNull(_menu.Skills);
    }

    private void OpenItems()
    {
        _inventory.Add(BattleScenarios.Catalog.Items["potion"], 1);
        _inventory.Add(BattleScenarios.Catalog.Items["revive_feather"], 1);
        _menu.Start(_battle);
        Select(CommandEntry.Items);
        Press(GameButton.Confirm);
        Assert.NotNull(_menu.Items);
    }

    private void Select(CommandEntry entry)
    {
        for (var presses = 0; presses < CommandMenu.Entries.Length && _menu.Selected != entry; presses++)
        {
            Press(GameButton.Down);
        }

        Assert.Equal(entry, _menu.Selected);
    }
}
