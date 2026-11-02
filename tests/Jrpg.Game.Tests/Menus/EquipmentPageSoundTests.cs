using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// The slots, the member, a slot's choice, and what Confirm carries out (decision 421).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EquipmentPageSoundTests
{
    private readonly GameSession _session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    [Fact]
    public void Update_DownOnTheSlots_AsksTheMoveSound()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.Down));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], page.Sounds);
    }

    [Fact]
    public void Update_NextMember_AsksTheMoveSound()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.NextTab));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], page.Sounds);
    }

    [Fact]
    public void Update_ConfirmOnASlot_AsksTheConfirmSound()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.Confirm));

        Assert.False(page.LeavesOnCancel);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], page.Sounds);
    }

    [Fact]
    public void Update_DownInTheChoice_AsksTheMoveSound()
    {
        var page = PageChoosingAWeapon();

        page.Update(Pressing(GameButton.Down));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], page.Sounds);
    }

    [Fact]
    public void Update_CancelInTheChoice_AsksTheCancelSound()
    {
        var page = PageChoosingAWeapon();

        page.Update(Pressing(GameButton.Cancel));

        Assert.True(page.LeavesOnCancel);
        Assert.Equal([UiSoundFiles.Effects.MenuCancel], page.Sounds);
    }

    [Fact]
    public void Update_ConfirmInTheChoice_AsksTheConfirmSound()
    {
        var page = PageChoosingAWeapon();

        page.Update(Pressing(GameButton.Confirm));

        Assert.True(page.LeavesOnCancel);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], page.Sounds);
    }

    [Fact]
    public void Update_NothingPressed_AsksNothing()
    {
        var page = PageChoosingAWeapon();

        page.Update(default);

        Assert.Empty(page.Sounds);
    }

    private EquipmentPage NewPage()
    {
        var page = new EquipmentPage(RepositoryScreenText.Text, new UiTextures(), _session);
        page.Open();
        return page;
    }

    // A loose sword, so the weapon's choice has a row past Remove.
    private EquipmentPage PageChoosingAWeapon()
    {
        _session.Inventory.AddGear(GearPiece.Plain(StartMapScenarios.BelowClosedDoor.Items["bronze_sword"]));
        var page = NewPage();
        EquipmentPageRows.MoveDown(page, EquipmentPageRows.Of(EquipmentSlot.Weapon));
        page.Update(Pressing(GameButton.Confirm));
        Assert.False(page.LeavesOnCancel);
        return page;
    }
}
