using Jrpg.Core.Effects;
using Jrpg.Core.Party;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

public sealed class ItemTargetPickerTests
{
    private readonly IReadOnlyList<PartyMember> _members =
        StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor).Party.Members;

    [Fact]
    public void Update_ConfirmOnTakesAllChoice_GivesEveryCandidate()
    {
        var candidates = new[] { _members[0], _members[2] };
        var picker = new ItemTargetPicker(_members, new TargetChoice<PartyMember>(candidates, TakesAll: true));

        var targets = picker.Update(Pressing(GameButton.Confirm));

        Assert.Equal(candidates, targets);
        Assert.True(picker.IsSelected(_members[2]));
        Assert.False(picker.IsSelected(_members[1]));
    }

    [Fact]
    public void Update_MoveDown_SkipsMembersOutsideCandidates()
    {
        var picker = new ItemTargetPicker(_members, new TargetChoice<PartyMember>([_members[0], _members[2]], TakesAll: false));

        picker.Update(Pressing(GameButton.Down));
        var targets = picker.Update(Pressing(GameButton.Confirm));

        Assert.Equal([_members[2]], targets);
    }

    [Fact]
    public void Update_NothingPressed_GivesNoTargets()
    {
        var picker = new ItemTargetPicker(_members, new TargetChoice<PartyMember>([_members[1]], TakesAll: false));

        Assert.Null(picker.Update(default));
        Assert.True(picker.IsSelected(_members[1]));
    }

    // The cards are one column (decision 267), so Left and Right keep the target, and Left never reaches the rail.
    [Fact]
    public void Update_Right_KeepsTheTarget()
    {
        var picker = new ItemTargetPicker(_members, new TargetChoice<PartyMember>([_members[0], _members[2]], TakesAll: false));

        picker.Update(Pressing(GameButton.Right));

        Assert.True(picker.IsSelected(_members[0]));
    }

    [Fact]
    public void Update_Left_KeepsTheTarget()
    {
        var picker = new ItemTargetPicker(_members, new TargetChoice<PartyMember>([_members[0], _members[2]], TakesAll: false));

        picker.Update(Pressing(GameButton.Left));

        Assert.True(picker.IsSelected(_members[0]));
    }

    [Fact]
    public void Pointed_TakesAllChoice_IsNull()
    {
        var picker = new ItemTargetPicker(_members, new TargetChoice<PartyMember>([_members[0], _members[2]], TakesAll: true));

        Assert.Null(picker.Pointed);
    }

    [Fact]
    public void Pointed_AfterDown_IsTheNextCandidate()
    {
        var picker = new ItemTargetPicker(_members, new TargetChoice<PartyMember>([_members[0], _members[2]], TakesAll: false));

        picker.Update(Pressing(GameButton.Down));

        Assert.Same(_members[2], picker.Pointed);
    }
}
