using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Input;

namespace Jrpg.Game.Tests.Battle;

// The Skills entry. Zanyr, a cleric, knows her class's skills, in order: Heal, Recovery, Heal All; the seed named for
// Sarxian puts her first here too.
public sealed class CommandMenuSkillTests
{
    private const int RecoveryRow = 1;
    private const int HealAllRow = 2;

    private readonly Inventory _inventory = new();
    private readonly BattleState _battle;
    private readonly CommandMenu _menu;

    public CommandMenuSkillTests()
    {
        _battle = BattleScenarios.MemberAgainstGoblinTrio("zanyr", _inventory, new SeededRandom(BattleScenarios.SarxianFirstSeed));
        _menu = new CommandMenu(_inventory, BattleScenarios.Catalog.Items);
        _menu.Start(_battle);
    }

    // Every class in content has skills, so the member is Zaruk with his class's skill list emptied.
    [Fact]
    public void IsEnabled_ActorWithoutSkills_GreysSkills()
    {
        var menu = new CommandMenu(_inventory, BattleScenarios.Catalog.Items);
        var definition = BattleScenarios.Catalog.PartyMembers["zaruk"];
        var member = PartyMember.Recruit(definition, BattleScenarios.Catalog.ClassOf(definition) with { SkillIds = [] }, BattleScenarios.Catalog.Skills, BattleScenarios.Catalog.Rules.Stats);
        var party = new PartyState([member], [member], BattleScenarios.Catalog.Rules.Party);
        var goblin = BattleScenarios.Catalog.Enemies["goblin"];

        menu.Start(BattleState.Start(party, [goblin], _inventory, new SeededRandom(BattleScenarios.SarxianFirstSeed)));

        Assert.False(menu.IsEnabled(CommandEntry.Skills));
    }

    [Fact]
    public void Update_ConfirmSkills_ListsActorSkills()
    {
        OpenSkills();

        Assert.Equal(_battle.CurrentActor.Skills, _menu.Skills?.Skills);
    }

    // Omer starts with 3 mana; Meteor costs 9.
    [Fact]
    public void Update_ConfirmMeteorWithThreeMana_SaysNotEnoughMana()
    {
        var menu = OmersSkillList();
        SelectSkill(menu, "meteor");

        menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.False(menu.Skills?.IsUsable(menu.Skills.SelectedIndex));
        Assert.True(menu.Skills?.IsSayingCannotAfford);
        Assert.Equal("Mana", menu.Skills?.LackingResourceAt(menu.Skills.SelectedIndex)?.Name);
        Assert.Null(menu.Targets);
    }

    [Fact]
    public void Update_ConfirmSelfSkill_ProducesCommandOnActor()
    {
        OpenSkills();
        CommandMenuDriver.Press(_menu, GameButton.Down, times: RecoveryRow);
        _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        var command = _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        var skill = Assert.IsType<SkillCommand>(command);
        Assert.Equal("recovery", skill.Skill.Id);
        Assert.Equal([_battle.CurrentActor], skill.Targets);
    }

    [Fact]
    public void Update_AllAlliesSkill_ProducesCommandOnEveryLivingMember()
    {
        OpenSkills();
        CommandMenuDriver.Press(_menu, GameButton.Down, times: HealAllRow);
        _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        var command = _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        var skill = Assert.IsType<SkillCommand>(command);
        Assert.Equal("heal_all", skill.Skill.Id);
        Assert.Equal(_battle.LivingOn(BattleSide.Party), skill.Targets);
    }

    [Fact]
    public void Update_CancelOnSkillList_ReturnsToCommands()
    {
        OpenSkills();

        _menu.Update(InputFrames.Pressing(GameButton.Cancel));

        Assert.Null(_menu.Skills);
        Assert.Equal(CommandEntry.Skills, _menu.Selected);
    }

    [Fact]
    public void Update_CancelOnSkillTargets_ReturnsToSkillList()
    {
        OpenSkills();
        _menu.Update(InputFrames.Pressing(GameButton.Confirm));

        _menu.Update(InputFrames.Pressing(GameButton.Cancel));

        Assert.Null(_menu.Targets);
        Assert.Equal("heal", _menu.Skills?.Selected.Id);
    }

    [Fact]
    public void Update_ConfirmReady_TargetsOthersButNotSelf()
    {
        var battle = BattleScenarios.MemberAgainstGoblinTrio("lazarus", _inventory, new SeededRandom(BattleScenarios.SarxianFirstSeed));
        var menu = new CommandMenu(_inventory, BattleScenarios.Catalog.Items);
        menu.Start(battle);
        CommandMenuDriver.SelectEntry(menu, CommandEntry.Skills);
        menu.Update(InputFrames.Pressing(GameButton.Confirm));
        SelectSkill(menu, "ready");

        menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.Equal(battle.LivingOn(BattleSide.Enemies), menu.Targets?.Candidates);
        Assert.DoesNotContain(battle.CurrentActor, menu.Targets!.Candidates);
    }

    private void OpenSkills()
    {
        _menu.Update(InputFrames.Pressing(GameButton.Down));
        _menu.Update(InputFrames.Pressing(GameButton.Confirm));
    }

    // Omer, a mage, acts first with Skills open.
    private CommandMenu OmersSkillList()
    {
        var menu = new CommandMenu(_inventory, BattleScenarios.Catalog.Items);
        menu.Start(BattleScenarios.MemberAgainstGoblinTrio("omer", _inventory, new SeededRandom(BattleScenarios.SarxianFirstSeed)));
        menu.Update(InputFrames.Pressing(GameButton.Down));
        menu.Update(InputFrames.Pressing(GameButton.Confirm));
        return menu;
    }

    // Bounded by the rows, so a list without the skill fails the test instead of looping forever.
    private static void SelectSkill(CommandMenu menu, string skillId)
    {
        var skills = menu.Skills!;
        for (var presses = 0; presses < skills.Skills.Count && skills.Selected.Id != skillId; presses++)
        {
            menu.Update(InputFrames.Pressing(GameButton.Down));
        }

        Assert.Equal(skillId, skills.Selected.Id);
    }
}
