using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

public sealed class EquipmentChoiceTests
{
    private static readonly IReadOnlyDictionary<string, Core.Items.ItemDefinition> _items = StartMapScenarios.BelowClosedDoor.Items;

    private readonly PartyState _party = GameSession.StartNew(StartMapScenarios.BelowClosedDoor, new SeededRandom(1)).Party;

    [Fact]
    public void Update_ConfirmFirstRow_PicksRemove()
    {
        var choice = new EquipmentChoice([_items["bronze_sword"]], []);

        var pick = choice.Update(Pressing(GameButton.Confirm));

        Assert.NotNull(pick);
        Assert.Null(pick.Item);
    }

    [Fact]
    public void Update_ConfirmAfterDown_PicksTheFirstItem()
    {
        var choice = new EquipmentChoice([_items["bronze_sword"], _items["iron_sword"]], []);
        choice.Update(Pressing(GameButton.Down));

        var pick = choice.Update(Pressing(GameButton.Confirm));

        Assert.Same(_items["bronze_sword"], pick?.Item);
        Assert.Same(_items["bronze_sword"], choice.Highlighted);
    }

    [Fact]
    public void Rows_WornByOthers_ListedAfterTheOwnedOnes()
    {
        var wearer = _party.MemberById("zanyr");
        var choice = new EquipmentChoice([_items["bronze_sword"]], [new WornItem(wearer, _items["iron_sword"])]);

        choice.Update(Pressing(GameButton.Down));
        choice.Update(Pressing(GameButton.Down));

        Assert.Equal(3, choice.RowCount);
        Assert.Same(_items["iron_sword"], choice.Highlighted);
        Assert.Same(wearer, choice.HighlightedWearer);
    }

    [Fact]
    public void Update_ConfirmWornRow_PicksItWithItsWearer()
    {
        var wearer = _party.MemberById("zanyr");
        var choice = new EquipmentChoice([], [new WornItem(wearer, _items["iron_sword"])]);
        choice.Update(Pressing(GameButton.Down));

        var pick = choice.Update(Pressing(GameButton.Confirm));

        Assert.Equal(new EquipmentPick(_items["iron_sword"], wearer), pick);
    }

    [Fact]
    public void Update_ConfirmOwnedRow_HasNoGiver()
    {
        var choice = new EquipmentChoice([_items["bronze_sword"]], [new WornItem(_party.MemberById("zanyr"), _items["iron_sword"])]);
        choice.Update(Pressing(GameButton.Down));

        var pick = choice.Update(Pressing(GameButton.Confirm));

        Assert.Equal(new EquipmentPick(_items["bronze_sword"], Giver: null), pick);
    }

    [Fact]
    public void StatPreview_Of_EmptySlotItem_ShowsFullBonus()
    {
        var sword = _items["bronze_sword"];

        var preview = StatPreview.Of(_party.Leader, sword);

        Assert.Equal(sword.Equipment!.AttackBonus, preview.AttackChange);
        Assert.Equal(0, preview.DefenseChange);
    }

    [Fact]
    public void StatPreview_Of_StrongerWeapon_ShowsPositiveAttack()
    {
        var session = GameSession.StartNew(StartMapScenarios.BelowClosedDoor, new SeededRandom(1));
        var leader = session.Party.Leader;
        session.Equipment.TryEquip(leader, _items["bronze_sword"]);

        var preview = StatPreview.Of(leader, _items["iron_sword"]);

        Assert.Equal(_items["iron_sword"].Equipment!.AttackBonus - _items["bronze_sword"].Equipment!.AttackBonus, preview.AttackChange);
        Assert.True(preview.AttackChange > 0);
    }

    [Fact]
    public void StatPreview_OfRemoving_WornWeapon_ShowsTheBonusLost()
    {
        var session = GameSession.StartNew(StartMapScenarios.BelowClosedDoor, new SeededRandom(1));
        var leader = session.Party.Leader;
        session.Equipment.TryEquip(leader, _items["bronze_sword"]);

        var preview = StatPreview.OfRemoving(leader, Core.Items.EquipmentSlot.Weapon);

        Assert.Equal(-_items["bronze_sword"].Equipment!.AttackBonus, preview.AttackChange);
    }
}
