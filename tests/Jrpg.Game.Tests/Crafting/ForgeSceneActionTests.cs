using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Core.Stats;
using Jrpg.Game.Crafting;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Crafting;

// Confirm on a piece opens its actions, each quoted by Core: a refused one is greyed with its reason (decision 370).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ForgeSceneActionTests
{
    private static readonly IReadOnlyDictionary<string, ItemDefinition> _items = StartMapScenarios.BelowClosedDoor.Items;

    [Fact]
    public void Confirm_OnPiece_OpensActionsWithTheirMaterials()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        var sword = GearPiece.Plain(_items["bronze_sword"]);
        session.Inventory.AddGear(sword);
        var forge = NewForge(session);

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        var salvage = Assert.Single(forge.Actions!.Rows, row => row.Action == ForgeAction.Salvage);
        var yield = MaterialListText.Of(session.Forge.QuoteSalvage(sword).Materials, _items, RepositoryScreenText.Text.Messages);
        Assert.Equal(yield, salvage.Materials.Text);
        Assert.True(salvage.IsAvailable);
    }

    [Fact]
    public void Actions_RarePiece_UpgradeGreyedWithHighestRarity()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        session.Inventory.AddGear(GearPiece.Rolled(_items["bronze_sword"], Rarity.Rare, [new RolledAffix("power", StatKind.Power, 4)]));
        var forge = NewForge(session);

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        var upgrade = Assert.Single(forge.Actions!.Rows, row => row.Action == ForgeAction.Upgrade);
        Assert.Equal(RepositoryScreenText.Text.Messages.Text(MessageIds.ForgeRefusedHighestRarity), upgrade.Reason);
    }

    [Fact]
    public void Actions_PlainPiece_RerollGreyedWithNothingToReroll()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        session.Inventory.AddGear(GearPiece.Plain(_items["bronze_sword"]));
        var forge = NewForge(session);

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        var reroll = Assert.Single(forge.Actions!.Rows, row => row.Action == ForgeAction.Reroll);
        Assert.Equal(RepositoryScreenText.Text.Messages.Text(MessageIds.ForgeRefusedNoAffixes), reroll.Reason);
    }

    [Fact]
    public void Actions_NoMaterials_UpgradeGreyedShort()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        Assert.Equal(0, session.Inventory.QuantityOf("scrap"));
        session.Inventory.AddGear(GearPiece.Plain(_items["bronze_sword"]));
        var forge = NewForge(session);

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        var upgrade = Assert.Single(forge.Actions!.Rows, row => row.Action == ForgeAction.Upgrade);
        Assert.Equal(RepositoryScreenText.Text.Messages.Text(MessageIds.ForgeRefusedShort), upgrade.Reason);
        Assert.False(upgrade.IsAvailable);
    }

    // Cancel wins when nested: it closes the actions, and the Forge stays open.
    [Fact]
    public void Cancel_InActions_ClosesActionsOnly()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        session.Inventory.AddGear(GearPiece.Plain(_items["bronze_sword"]));
        var forge = NewForge(session);
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        var atCancel = forge.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));
        var requests = Enumerable.Range(0, ForgeSceneTests.ClosingFadeUpdates).Select(_ => forge.Update(StartMapScenarios.OneUpdate, default)).ToList();

        Assert.Null(forge.Actions);
        Assert.Null(atCancel);
        Assert.All(requests, Assert.Null);
    }

    [Fact]
    public void Down_InActions_MovesTheActionsNotTheList()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        session.Inventory.AddGear(GearPiece.Plain(_items["bronze_sword"]));
        session.Inventory.AddGear(GearPiece.Plain(_items["wooden_shield"]));
        var forge = NewForge(session);
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Down));

        Assert.Equal(ForgeAction.Upgrade, forge.Actions!.Selected.Action);
        Assert.Equal(session.Inventory.Gear[0], forge.SelectedPiece);
    }

    private static ForgeScene NewForge(GameSession session) =>
        new(RepositoryScreenText.Text, new UiTextures(), session, _items);
}
