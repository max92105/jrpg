using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// Moves, what Confirm carries out, and Cancel inside the page; refusals and their notices are silent (decision 421).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class PartyPageSoundTests
{
    private readonly GameSession _session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    [Fact]
    public void Update_RightOnTheCards_AsksTheMoveSound()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.Right));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], page.Sounds);
    }

    [Fact]
    public void Update_ConfirmOnAReserveMember_AsksTheConfirmSound()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Right));

        page.Update(Pressing(GameButton.Confirm));

        Assert.Equal(2, _session.Party.ActiveMembers.Count);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], page.Sounds);
    }

    [Fact]
    public void Update_DownInTheActions_AsksTheMoveSound()
    {
        var page = PageOnTheSecondOfThree();
        page.Update(Pressing(GameButton.Confirm));

        page.Update(Pressing(GameButton.Down));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], page.Sounds);
    }

    [Fact]
    public void Update_CancelInTheActions_AsksTheCancelSound()
    {
        var page = PageOnTheSecondOfThree();
        page.Update(Pressing(GameButton.Confirm));

        page.Update(Pressing(GameButton.Cancel));

        Assert.True(page.LeavesOnCancel);
        Assert.Equal([UiSoundFiles.Effects.MenuCancel], page.Sounds);
    }

    [Fact]
    public void Update_ConfirmASwap_AsksTheConfirmSound()
    {
        var page = PageOnTheSecondOfThree();
        var second = _session.Party.ActiveMembers[1];
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Confirm));
        Assert.Same(second, page.MovingMember);
        page.Update(Pressing(GameButton.Down));

        page.Update(Pressing(GameButton.Confirm));

        Assert.Same(second, _session.Party.ActiveMembers[2]);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], page.Sounds);
    }

    [Fact]
    public void Update_CancelInTheSwap_AsksTheCancelSound()
    {
        var page = PageOnTheSecondOfThree();
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Confirm));

        page.Update(Pressing(GameButton.Cancel));

        Assert.Null(page.MovingMember);
        Assert.Equal([UiSoundFiles.Effects.MenuCancel], page.Sounds);
    }

    // The leader always stays, so Confirm on the leader alone opens a notice.
    [Fact]
    public void Update_RefusalOpensANotice_AsksNothing()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.Confirm));

        Assert.NotNull(page.Notice);
        Assert.Empty(page.Sounds);
    }

    [Fact]
    public void Update_OkOnTheNotice_AsksNothing()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Confirm));

        page.Update(Pressing(GameButton.Confirm));

        Assert.Null(page.Notice);
        Assert.Empty(page.Sounds);
    }

    [Fact]
    public void Update_ConfirmOnAnActiveMember_AsksTheConfirmSound()
    {
        var page = PageOnTheSecondOfThree();

        page.Update(Pressing(GameButton.Confirm));

        Assert.False(page.LeavesOnCancel);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], page.Sounds);
    }

    [Fact]
    public void Update_ConfirmChangeOrder_AsksTheConfirmSound()
    {
        var page = PageOnTheSecondOfThree();
        page.Update(Pressing(GameButton.Confirm));

        page.Update(Pressing(GameButton.Confirm));

        Assert.NotNull(page.MovingMember);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], page.Sounds);
    }

    [Fact]
    public void Update_ConfirmMoveToReserve_AsksTheConfirmSound()
    {
        var page = PageOnTheSecondOfThree();
        var leaving = page.Selected;
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Down));

        page.Update(Pressing(GameButton.Confirm));

        Assert.DoesNotContain(leaving, _session.Party.ActiveMembers);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], page.Sounds);
    }

    // The fallen leader leaves the other as the only one standing, who can't leave the party.
    [Fact]
    public void Update_MoveToReserveRefused_AsksNothing()
    {
        Assert.True(_session.Party.TryAddToActive(_session.Party.MemberById("teelle")));
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));
        _session.Party.Leader.Vitals.TakeDamage(_session.Party.Leader.Vitals.MaxHp);
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Down));

        page.Update(Pressing(GameButton.Confirm));

        Assert.NotNull(page.Notice);
        Assert.Empty(page.Sounds);
    }

    [Fact]
    public void Update_JoinOnAFullParty_AsksNothing()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Right));
        for (var added = 0; added < 4; added++)
        {
            page.Update(Pressing(GameButton.Confirm));
        }

        page.Update(Pressing(GameButton.Confirm));

        Assert.NotNull(page.Notice);
        Assert.Empty(page.Sounds);
    }

    private PartyPage NewPage()
    {
        var page = new PartyPage(RepositoryScreenText.Text, new UiTextures(), _session, StartMapScenarios.BelowClosedDoor);
        page.Open();
        return page;
    }

    private PartyPage PageOnTheSecondOfThree()
    {
        Assert.True(_session.Party.TryAddToActive(_session.Party.MemberById("teelle")));
        Assert.True(_session.Party.TryAddToActive(_session.Party.MemberById("jazog")));
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));
        Assert.Same(_session.Party.ActiveMembers[1], page.Selected);
        return page;
    }
}
