using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class EquipmentPageTests
{
    private readonly GameSession _session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    [Fact]
    public void Update_NextMemberOnLastMember_WrapsToFirst()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.PreviousTab));
        var last = page.Shown;

        page.Update(Pressing(GameButton.NextTab));

        Assert.Same(page.Members[^1], last);
        Assert.Same(page.Members[0], page.Shown);
    }

    [Fact]
    public void Update_RemoveAll_EmptiesEverySlot()
    {
        var sarxian = WearingSwordAndArmor();
        Assert.True(_session.Equipment.TryEquip(sarxian, StartMapScenarios.BelowClosedDoor.Items["power_ring"], EquipmentSlot.SecondRing));
        var page = NewPage();

        ConfirmRemoveAll(page);

        Assert.All(EquipmentSheet.Slots, slot => Assert.Null(sarxian.EquippedIn(slot)));
        Assert.Equal(2, _session.Inventory.GearCountOf("bronze_sword"));
        Assert.Equal(2, _session.Inventory.GearCountOf("leather_armor"));
        Assert.Equal(1, _session.Inventory.GearCountOf("power_ring"));
    }

    // The new game's bag holds a power ring among its rings; the second ring row is the last slot row.
    [Fact]
    public void Update_ConfirmOnSecondRing_ListsOwnedRings()
    {
        var page = NewPage();
        EquipmentPageRows.MoveDown(page, EquipmentPageRows.Of(EquipmentSlot.SecondRing));

        page.Update(Pressing(GameButton.Confirm));

        Assert.True(page.IsChoosing);
        Assert.Contains(StartMapScenarios.BelowClosedDoor.Items["power_ring"], page.Choice!.Items);
        Assert.All(page.Choice.Items, item => Assert.Equal(EquipmentKind.Ring, item.Equipment!.Kind));
    }

    [Fact]
    public void Update_Right_ShowsTheNextMember()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.Right));

        Assert.Same(page.Members[1], page.Shown);
    }

    [Fact]
    public void Open_AfterMemberJoinedElsewhere_ListsThemAmongTheActive()
    {
        var page = NewPage();
        var joining = _session.Party.MemberById("teelle");
        _session.Party.TryAddToActive(joining);

        page.Open();

        Assert.Same(joining, page.Members[1]);
    }

    [Fact]
    public void Update_ConfirmOnWeaponRow_OpensTheChoice()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.Confirm));

        Assert.True(page.IsChoosing);
        Assert.False(page.LeavesOnCancel);
    }

    [Fact]
    public void Update_ConfirmRemove_UnequipsSlot()
    {
        var sarxian = WearingSwordAndArmor();
        var page = NewPage();
        page.Update(Pressing(GameButton.Confirm));

        page.Update(Pressing(GameButton.Confirm));

        Assert.Null(sarxian.Weapon);
        Assert.False(page.IsChoosing);
        Assert.Equal(2, _session.Inventory.GearCountOf("bronze_sword"));
    }

    [Fact]
    public void Update_ChooseIronSword_SwapsItIn()
    {
        var sarxian = WearingSwordAndArmor();
        var page = NewPage();
        page.Update(Pressing(GameButton.Confirm));
        ChooseIronSword(page);

        page.Update(Pressing(GameButton.Confirm));

        Assert.Same(StartMapScenarios.BelowClosedDoor.Items["iron_sword"], sarxian.Weapon?.Base);
        Assert.Equal(2, _session.Inventory.GearCountOf("bronze_sword"));
    }

    [Fact]
    public void Update_ConfirmWornRow_MovesTheItemToTheShownMember()
    {
        var sarxian = WearingSwordAndArmor();
        var zanyr = ZanyrWearingIronSword();
        var page = NewPage();
        page.Update(Pressing(GameButton.Confirm));
        ChooseZanyrsIronSword(page);

        page.Update(Pressing(GameButton.Confirm));

        Assert.Same(StartMapScenarios.BelowClosedDoor.Items["iron_sword"], sarxian.Weapon?.Base);
        Assert.Null(zanyr.Weapon);
        Assert.Equal(2, _session.Inventory.GearCountOf("bronze_sword"));
    }

    [Fact]
    public void Hint_WornRowHighlighted_NamesTheWearer()
    {
        WearingSwordAndArmor();
        ZanyrWearingIronSword();
        var page = NewPage();
        page.Update(Pressing(GameButton.Confirm));

        ChooseZanyrsIronSword(page);

        Assert.Equal("Taking it leaves Zanyr without it.", page.Hint);
    }

    [Fact]
    public void Hint_CursorLeavesTheWornRow_IsThePageHintAgain()
    {
        WearingSwordAndArmor();
        ZanyrWearingIronSword();
        var page = NewPage();
        page.Update(Pressing(GameButton.Confirm));
        ChooseZanyrsIronSword(page);

        page.Update(Pressing(GameButton.Up));

        Assert.Equal(RepositoryScreenText.Text.Messages.Text(MessageIds.MenuHintEquipment), page.Hint);
    }

    // Zanyr, a knight in the reserve, wears the only iron sword.
    private PartyMember ZanyrWearingIronSword()
    {
        var zanyr = _session.Party.MemberById("zanyr");
        Assert.True(_session.Equipment.TryEquip(zanyr, StartMapScenarios.BelowClosedDoor.Items["iron_sword"]));
        return zanyr;
    }

    // Bounded by the rows, so a choice without Zanyr's sword fails the test instead of looping forever.
    private static void ChooseZanyrsIronSword(EquipmentPage page)
    {
        var choice = page.Choice!;
        for (var presses = 0; presses < choice.RowCount && !IsZanyrsIronSword(choice); presses++)
        {
            page.Update(Pressing(GameButton.Down));
        }

        Assert.True(IsZanyrsIronSword(choice));
    }

    private static bool IsZanyrsIronSword(EquipmentChoice choice) =>
        choice.HighlightedWearer?.Id == "zanyr" && choice.Highlighted?.Id == "iron_sword";

    [Fact]
    public void Advance_AfterMovingDown_SelectionReachesTheNextSlot()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));

        page.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal(1, page.SelectionPosition);
    }

    // Starts with 2 bronze swords and 2 leather armors in the inventory; the leader puts one of each on.
    private PartyMember WearingSwordAndArmor()
    {
        var sarxian = _session.Party.Leader;
        var items = StartMapScenarios.BelowClosedDoor.Items;
        Assert.True(_session.Equipment.TryEquip(sarxian, items["bronze_sword"]));
        Assert.True(_session.Equipment.TryEquip(sarxian, items["leather_armor"]));
        return sarxian;
    }

    // The weapon choice lists Remove, then the fighter's owned weapons in pickup order: bronze sword, iron sword.
    private static void ChooseIronSword(EquipmentPage page)
    {
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Down));
    }

    private static void ConfirmRemoveAll(EquipmentPage page)
    {
        EquipmentPageRows.MoveDown(page, EquipmentPageRows.RemoveAll);
        page.Update(Pressing(GameButton.Confirm));
    }

    private EquipmentPage NewPage() => new(RepositoryScreenText.Text, new UiTextures(), _session);
}
