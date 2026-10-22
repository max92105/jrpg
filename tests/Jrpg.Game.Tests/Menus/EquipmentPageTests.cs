using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
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
    private readonly GameSession _session = GameSession.StartNew(StartMapScenarios.BelowClosedDoor, new SeededRandom(1));

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
    public void Update_RemoveAll_EmptiesBothSlots()
    {
        var sarxian = WearingSwordAndArmor();
        var page = NewPage();

        ConfirmRemoveAll(page);

        Assert.Null(sarxian.Weapon);
        Assert.Null(sarxian.Armor);
        Assert.Equal(2, _session.Inventory.QuantityOf("bronze_sword"));
        Assert.Equal(2, _session.Inventory.QuantityOf("leather_armor"));
    }

    [Fact]
    public void Update_RemoveAllWithWeaponStackFull_KeepsTheWeaponWorn()
    {
        var sarxian = WearingSwordAndArmor();
        var sword = StartMapScenarios.BelowClosedDoor.Items["bronze_sword"];
        _session.Inventory.Add(sword, sword.MaxStack - _session.Inventory.QuantityOf(sword.Id));
        var page = NewPage();

        ConfirmRemoveAll(page);

        Assert.Same(sword, sarxian.Weapon);
        Assert.Null(sarxian.Armor);
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
    public void ShowsPartySidebar_WhileChoiceOpen_IsFalse()
    {
        var page = NewPage();
        var beforeChoosing = page.ShowsPartySidebar;

        page.Update(Pressing(GameButton.Confirm));

        Assert.True(beforeChoosing);
        Assert.False(page.ShowsPartySidebar);
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
        Assert.Equal(2, _session.Inventory.QuantityOf("bronze_sword"));
    }

    [Fact]
    public void Update_ChooseIronSword_SwapsItIn()
    {
        var sarxian = WearingSwordAndArmor();
        var page = NewPage();
        page.Update(Pressing(GameButton.Confirm));
        ChooseIronSword(page);

        page.Update(Pressing(GameButton.Confirm));

        Assert.Same(StartMapScenarios.BelowClosedDoor.Items["iron_sword"], sarxian.Weapon);
        Assert.Equal(2, _session.Inventory.QuantityOf("bronze_sword"));
    }

    [Fact]
    public void Update_ChooseIronSwordWithBronzeStackFull_RefusesAndSaysWhy()
    {
        var sarxian = WearingSwordAndArmor();
        var bronze = StartMapScenarios.BelowClosedDoor.Items["bronze_sword"];
        _session.Inventory.Add(bronze, bronze.MaxStack - _session.Inventory.QuantityOf(bronze.Id));
        var page = NewPage();
        page.Update(Pressing(GameButton.Confirm));
        ChooseIronSword(page);

        page.Update(Pressing(GameButton.Confirm));

        Assert.Same(bronze, sarxian.Weapon);
        Assert.Equal($"There's no room in the bag for {bronze.Name}.", page.Notice);
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

        Assert.Same(StartMapScenarios.BelowClosedDoor.Items["iron_sword"], sarxian.Weapon);
        Assert.Null(zanyr.Weapon);
        Assert.Equal(2, _session.Inventory.QuantityOf("bronze_sword"));
    }

    [Fact]
    public void Update_TakeWithNoRoomForOwnItem_ShowsTheNoRoomNotice()
    {
        var sarxian = WearingSwordAndArmor();
        var zanyr = ZanyrWearingIronSword();
        var bronze = StartMapScenarios.BelowClosedDoor.Items["bronze_sword"];
        _session.Inventory.Add(bronze, bronze.MaxStack - _session.Inventory.QuantityOf(bronze.Id));
        var page = NewPage();
        page.Update(Pressing(GameButton.Confirm));
        ChooseZanyrsIronSword(page);

        page.Update(Pressing(GameButton.Confirm));

        Assert.Same(bronze, sarxian.Weapon);
        Assert.NotNull(zanyr.Weapon);
        Assert.Equal($"There's no room in the bag for {bronze.Name}.", page.Notice);
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

    // With the leader wearing a bronze sword and Zanyr the iron one, the weapon choice lists Remove, the other bronze
    // sword, then Zanyr's iron sword.
    private static void ChooseZanyrsIronSword(EquipmentPage page)
    {
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Down));
    }

    // Starts with 2 bronze swords and 2 leather armors in the inventory; the leader, a warrior, can wear both.
    private PartyMember WearingSwordAndArmor()
    {
        var sarxian = _session.Party.Leader;
        var items = StartMapScenarios.BelowClosedDoor.Items;
        Assert.True(_session.Equipment.TryEquip(sarxian, items["bronze_sword"]));
        Assert.True(_session.Equipment.TryEquip(sarxian, items["leather_armor"]));
        return sarxian;
    }

    // The weapon choice lists Remove, then the warrior's owned weapons in pickup order: the bronze sword, the iron sword.
    private static void ChooseIronSword(EquipmentPage page)
    {
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Down));
    }

    // The rows are Weapon, Armor, then Remove all.
    private static void ConfirmRemoveAll(EquipmentPage page)
    {
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Confirm));
    }

    private EquipmentPage NewPage() => new(RepositoryScreenText.Text, new UiTextures(), _session, StartMapScenarios.BelowClosedDoor);
}
