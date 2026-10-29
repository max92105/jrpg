using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Game.Crafting;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Crafting;

// Salvage asks first (decision 354); Yes breaks the piece into its yield through Core, No and Cancel keep it.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ForgeSceneSalvageTests
{
    private static readonly IReadOnlyDictionary<string, ItemDefinition> _items = StartMapScenarios.BelowClosedDoor.Items;

    [Fact]
    public void Confirm_Salvage_AsksFirst()
    {
        var (forge, session, sword) = ForgeOnSalvage();

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.NotNull(forge.Prompt);
        Assert.Contains(sword, session.Inventory.Gear);
    }

    [Fact]
    public void PromptYes_Salvages_PieceGoneMaterialsUp()
    {
        var (forge, session, sword) = ForgeOnSalvage();
        var yield = session.Forge.QuoteSalvage(sword).Materials;
        var scrapBefore = session.Inventory.QuantityOf("scrap");
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Up));
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.DoesNotContain(sword, session.Inventory.Gear);
        Assert.Equal(scrapBefore + yield.Single(entry => entry.ItemId == "scrap").Quantity, session.Inventory.QuantityOf("scrap"));
        Assert.Null(forge.Prompt);
        Assert.Null(forge.Actions);
        Assert.Equal(RepositoryScreenText.Text.Messages.Format(MessageIds.ForgeSalvaged, new MessageArgument("materials", MaterialListText.Of(yield, _items, RepositoryScreenText.Text.Messages))), forge.Notice);
    }

    // The prompt starts on No, so a stray Confirm keeps the piece.
    [Fact]
    public void PromptNo_KeepsPiece()
    {
        var (forge, session, sword) = ForgeOnSalvage();
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Contains(sword, session.Inventory.Gear);
        Assert.Null(forge.Prompt);
        Assert.NotNull(forge.Actions);
    }

    [Fact]
    public void Cancel_InPrompt_ClosesThePromptOnly()
    {
        var (forge, _, _) = ForgeOnSalvage();
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));

        Assert.Null(forge.Prompt);
        Assert.NotNull(forge.Actions);
    }

    // A full Scrap stack makes Core refuse the salvage (decision 358): no prompt, nothing changes.
    [Fact]
    public void Confirm_GreyedSalvage_DoesNothing()
    {
        var (forge, session, sword) = ForgeOnSalvage(scrapOwned: _items["scrap"].MaxStack);
        Assert.False(forge.Actions!.Selected.IsAvailable);

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Null(forge.Prompt);
        Assert.Contains(sword, session.Inventory.Gear);
        Assert.Equal(_items["scrap"].MaxStack, session.Inventory.QuantityOf("scrap"));
    }

    // The notice says what the salvage gave until the cursor moves on.
    [Fact]
    public void Down_AfterSalvage_ClearsTheNotice()
    {
        var (forge, session, _) = ForgeOnSalvage();
        session.Inventory.AddGear(GearPiece.Plain(_items["wooden_shield"]));
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Up));
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        Assert.NotNull(forge.Notice);

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Down));

        Assert.Null(forge.Notice);
    }

    // The cursor stays on its row, now the next piece, so a run of salvages walks down the list.
    [Fact]
    public void PromptYes_OnAMiddleRow_KeepsTheCursorOnTheRow()
    {
        var (forge, session, shield, cap) = ForgeOnThreePieces();
        SalvageHighlighted(forge, shield);

        Assert.Equal(cap, forge.SelectedPiece);
        Assert.DoesNotContain(shield, session.Inventory.Gear);
    }

    [Fact]
    public void PromptYes_OnTheLastRow_MovesToTheNewLast()
    {
        var (forge, _, shield, cap) = ForgeOnThreePieces();
        SalvageHighlighted(forge, cap);

        Assert.Equal(shield, forge.SelectedPiece);
    }

    // Opening another piece's actions hides the notice, so Cancel can't bring back a line about a piece that is gone.
    [Fact]
    public void Confirm_OnAPieceAfterSalvage_ClearsTheNotice()
    {
        var (forge, _, shield, _) = ForgeOnThreePieces();
        SalvageHighlighted(forge, shield);
        Assert.NotNull(forge.Notice);

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Null(forge.Notice);
    }

    // Moves to the piece, opens its actions on Salvage and answers Yes.
    private static void SalvageHighlighted(ForgeScene forge, GearPiece piece)
    {
        while (forge.SelectedPiece != piece)
        {
            forge.Update(TimeSpan.Zero, Pressing(GameButton.Down));
        }

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Up));
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
    }

    // A Forge with a sword, a shield and a cap owned, in that order, its cursor on the sword.
    private static (ForgeScene Forge, GameSession Session, GearPiece Shield, GearPiece Cap) ForgeOnThreePieces()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        var shield = GearPiece.Plain(_items["wooden_shield"]);
        var cap = GearPiece.Plain(_items["leather_cap"]);
        session.Inventory.AddGear(GearPiece.Plain(_items["bronze_sword"]));
        session.Inventory.AddGear(shield);
        session.Inventory.AddGear(cap);
        return (new ForgeScene(RepositoryScreenText.Text, new UiTextures(), session, _items), session, shield, cap);
    }

    // A Forge with a plain sword owned, its actions open on Salvage (the first action).
    private static (ForgeScene Forge, GameSession Session, GearPiece Sword) ForgeOnSalvage(int scrapOwned = 0)
    {
        var session = ForgeSessions.WithoutLoosePieces();
        if (scrapOwned > 0)
        {
            session.Inventory.Add(_items["scrap"], scrapOwned);
        }

        var sword = GearPiece.Plain(_items["bronze_sword"]);
        session.Inventory.AddGear(sword);
        var forge = new ForgeScene(RepositoryScreenText.Text, new UiTextures(), session, _items);
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        Assert.Equal(ForgeAction.Salvage, forge.Actions!.Selected.Action);
        return (forge, session, sword);
    }
}
