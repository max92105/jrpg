using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Game.Crafting;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Crafting;

// Move, confirm and cancel at the Forge; prompts and refused actions are silent (decision 421).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ForgeSceneSoundTests
{
    private static readonly IReadOnlyDictionary<string, ItemDefinition> _items = StartMapScenarios.BelowClosedDoor.Items;

    [Fact]
    public void Update_DownInThePieces_AsksTheMoveSound()
    {
        var forge = NewForge(TwoPieces());

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Down));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], forge.Sounds);
    }

    [Fact]
    public void Update_ConfirmOnAPiece_AsksTheConfirmSound()
    {
        var forge = NewForge(TwoPieces());

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], forge.Sounds);
    }

    [Fact]
    public void Update_ConfirmWithNoPieces_AsksNothing()
    {
        var forge = NewForge(ForgeSessions.WithoutLoosePieces());

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Empty(forge.Sounds);
    }

    [Fact]
    public void Update_CancelInThePieces_AsksTheCancelSound()
    {
        var forge = NewForge(TwoPieces());

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));

        Assert.Equal([UiSoundFiles.Effects.MenuCancel], forge.Sounds);
    }

    [Fact]
    public void Update_DownInTheActions_AsksTheMoveSound()
    {
        var forge = NewForge(TwoPieces());
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Down));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], forge.Sounds);
    }

    [Fact]
    public void Update_CancelInTheActions_AsksTheCancelSound()
    {
        var forge = NewForge(TwoPieces());
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));

        Assert.Equal([UiSoundFiles.Effects.MenuCancel], forge.Sounds);
    }

    // A plain piece has nothing to reroll, so that action is greyed.
    [Fact]
    public void Update_ConfirmOnARefusedAction_AsksNothing()
    {
        var forge = ForgeOn(TwoPieces(), ForgeAction.Reroll);

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Empty(forge.Sounds);
    }

    [Fact]
    public void Update_UpgradeCarriedOut_AsksTheConfirmSound()
    {
        var session = TwoPieces();
        session.Inventory.Add(_items["scrap"], 10);
        session.Inventory.Add(_items["essence"], 5);
        var forge = ForgeOn(session, ForgeAction.Upgrade);

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], forge.Sounds);
    }

    // Opening the prompt, and its Yes, are both silent: "Yes" starts on No, so Left moves to it.
    [Fact]
    public void Update_YesInTheSalvagePrompt_AsksNothing()
    {
        var forge = ForgeOn(TwoPieces(), ForgeAction.Salvage);

        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        var atTheQuestion = forge.Sounds.ToList();
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Left));
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Empty(atTheQuestion);
        Assert.Empty(forge.Sounds);
        Assert.Null(forge.Prompt);

        // A salvage closes the actions, a No would keep them open.
        Assert.Null(forge.Actions);
    }

    private static GameSession TwoPieces()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        session.Inventory.AddGear(GearPiece.Plain(_items["bronze_sword"]));
        session.Inventory.AddGear(GearPiece.Plain(_items["wooden_shield"]));
        return session;
    }

    private static ForgeScene NewForge(GameSession session) => new(RepositoryScreenText.Text, new UiTextures(), session, _items);

    private static ForgeScene ForgeOn(GameSession session, ForgeAction action)
    {
        var forge = NewForge(session);
        forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        for (var presses = 0; presses < 3 && forge.Actions!.Selected.Action != action; presses++)
        {
            forge.Update(TimeSpan.Zero, Pressing(GameButton.Down));
        }

        Assert.Equal(action, forge.Actions!.Selected.Action);
        return forge;
    }
}
