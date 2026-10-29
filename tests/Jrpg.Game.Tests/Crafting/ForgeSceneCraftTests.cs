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

// Upgrade and reroll go at once through Core; the highlight stays on the piece that changed and the actions open again
// on it. Seed-independent: the rarity and the count of affixes are asserted, never which affixes.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ForgeSceneCraftTests
{
    private static readonly IReadOnlyDictionary<string, ItemDefinition> _items = StartMapScenarios.BelowClosedDoor.Items;

    [Fact]
    public void Confirm_UpgradeAffordable_SameRowShowsMagicWithOneAffix()
    {
        var session = StockedSession();
        session.Inventory.AddGear(GearPiece.Plain(_items["bronze_sword"]));
        var forge = ForgeOn(session, ForgeAction.Upgrade);

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        var upgraded = forge.SelectedPiece!;
        Assert.Equal(Rarity.Magic, upgraded.Rarity);
        Assert.Single(upgraded.Affixes);
        Assert.Equal(upgraded, session.Inventory.Gear[0]);
        Assert.Contains(forge.Actions!.Rows, row => row.Action == ForgeAction.Reroll && row.IsAvailable);
        var messages = RepositoryScreenText.Text.Messages;
        Assert.Equal(messages.Format(MessageIds.ForgeUpgraded, new MessageArgument("name", upgraded.Base.Name), new MessageArgument("rarity", messages.Text(MessageIds.RarityMagic))), forge.Notice);
    }

    // Two equal plain pieces: Core replaces the first, so the highlight moves to the first row, which holds the Magic
    // one.
    [Fact]
    public void Confirm_UpgradeOnSecondOfTwoEqualPieces_HighlightsTheUpgradedPiece()
    {
        var session = StockedSession();
        session.Inventory.AddGear(GearPiece.Plain(_items["bronze_sword"]));
        session.Inventory.AddGear(GearPiece.Plain(_items["bronze_sword"]));
        var forge = new ForgeScene(RepositoryScreenText.Text, new UiTextures(), session, _items);
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Down));
        OpenActionsOn(forge, ForgeAction.Upgrade);

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Equal(Rarity.Magic, session.Inventory.Gear[0].Rarity);
        Assert.Equal(Rarity.Common, session.Inventory.Gear[1].Rarity);
        Assert.Equal(session.Inventory.Gear[0], forge.SelectedPiece);
    }

    [Fact]
    public void Confirm_RerollAffordable_CostTakenAndLinesRebuilt()
    {
        var session = StockedSession();
        var piece = GearPiece.Rolled(_items["bronze_sword"], Rarity.Magic, [new RolledAffix("power", StatKind.Power, 4)]);
        session.Inventory.AddGear(piece);
        var cost = StartMapScenarios.BelowClosedDoor.Rules.Crafting.RerollCost;
        var before = cost.ToDictionary(entry => entry.ItemId, entry => session.Inventory.QuantityOf(entry.ItemId));
        var forge = ForgeOn(session, ForgeAction.Reroll);

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.All(cost, entry => Assert.Equal(before[entry.ItemId] - entry.Quantity, session.Inventory.QuantityOf(entry.ItemId)));
        var rerolled = forge.SelectedPiece!;
        Assert.Equal(Rarity.Magic, rerolled.Rarity);
        Assert.Single(rerolled.Affixes);
        Assert.Equal(rerolled, session.Inventory.Gear[0]);
        Assert.Equal(RepositoryScreenText.Text.Messages.Format(MessageIds.ForgeRerolled, new MessageArgument("name", rerolled.Base.Name)), forge.Notice);
    }

    // A Rare piece is at the top tier a craft reaches: Core refuses, nothing changes and nothing is drawn.
    [Fact]
    public void Confirm_GreyedUpgrade_DoesNothing()
    {
        var session = StockedSession();
        var piece = GearPiece.Rolled(_items["bronze_sword"], Rarity.Rare, [new RolledAffix("power", StatKind.Power, 4)]);
        session.Inventory.AddGear(piece);
        var scrap = session.Inventory.QuantityOf("scrap");
        var forge = ForgeOn(session, ForgeAction.Upgrade);
        Assert.False(forge.Actions!.Selected.IsAvailable);

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Equal(piece, session.Inventory.Gear[0]);
        Assert.Equal(scrap, session.Inventory.QuantityOf("scrap"));
        Assert.Null(forge.Notice);
        Assert.NotNull(forge.Actions);
    }

    // Enough Scrap and Essence for any one action.
    private static GameSession StockedSession()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        session.Inventory.Add(_items["scrap"], 10);
        session.Inventory.Add(_items["essence"], 5);
        return session;
    }

    private static ForgeScene ForgeOn(GameSession session, ForgeAction action)
    {
        var forge = new ForgeScene(RepositoryScreenText.Text, new UiTextures(), session, _items);
        OpenActionsOn(forge, action);
        return forge;
    }

    private static void OpenActionsOn(ForgeScene forge, ForgeAction action)
    {
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        while (forge.Actions!.Selected.Action != action)
        {
            forge.Update(TimeSpan.Zero, Pressing(GameButton.Down));
        }
    }
}
