using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// The list starts on the potion, which anyone standing can take; a refused Confirm is silent (decision 421).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class InventoryPageSoundTests
{
    private readonly InventoryPage _page = new(
        RepositoryScreenText.Text,
        new UiTextures(),
        StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor),
        StartMapScenarios.BelowClosedDoor);

    public InventoryPageSoundTests() => _page.Open();

    [Fact]
    public void Update_DownInTheList_AsksTheMoveSound()
    {
        _page.Update(Pressing(GameButton.Down));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], _page.Sounds);
    }

    [Fact]
    public void Update_LeftOntoTheRail_AsksTheMoveSound()
    {
        _page.Update(Pressing(GameButton.Left));

        Assert.True(_page.IsChoosingCategory);
        Assert.Equal([UiSoundFiles.Effects.MenuMove], _page.Sounds);
    }

    [Fact]
    public void Update_DownOnTheRail_AsksTheMoveSound()
    {
        _page.Update(Pressing(GameButton.Left));
        var category = _page.Category;

        _page.Update(Pressing(GameButton.Down));

        Assert.NotEqual(category, _page.Category);
        Assert.Equal([UiSoundFiles.Effects.MenuMove], _page.Sounds);
    }

    [Fact]
    public void Update_NextTabInTheList_AsksTheMoveSound()
    {
        var category = _page.Category;

        _page.Update(Pressing(GameButton.NextTab));

        Assert.NotEqual(category, _page.Category);
        Assert.Equal([UiSoundFiles.Effects.MenuMove], _page.Sounds);
    }

    [Fact]
    public void Update_ConfirmAPotion_AsksTheConfirmSound()
    {
        _page.Update(Pressing(GameButton.Confirm));

        Assert.True(_page.IsChoosingTarget);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], _page.Sounds);
    }

    [Fact]
    public void Update_ConfirmUsesAPotion_AsksTheConfirmSound()
    {
        _page.Update(Pressing(GameButton.Confirm));

        _page.Update(Pressing(GameButton.Confirm));

        Assert.NotNull(_page.Notice);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], _page.Sounds);
    }

    [Fact]
    public void Update_DownInTheTargetPicker_AsksTheMoveSound()
    {
        _page.Update(Pressing(GameButton.Confirm));

        _page.Update(Pressing(GameButton.Down));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], _page.Sounds);
    }

    [Fact]
    public void Update_CancelInTheTargetPicker_AsksTheCancelSound()
    {
        _page.Update(Pressing(GameButton.Confirm));

        _page.Update(Pressing(GameButton.Cancel));

        Assert.False(_page.IsChoosingTarget);
        Assert.Equal([UiSoundFiles.Effects.MenuCancel], _page.Sounds);
    }

    // The bomb can't be used from the menu, so its row is greyed.
    [Fact]
    public void Update_ConfirmOnAGreyedRow_AsksNothing()
    {
        for (var presses = 0; presses < _page.ShownItemIds.Count && _page.ShownItemIds[_page.SelectedRow ?? 0] != "bomb"; presses++)
        {
            _page.Update(Pressing(GameButton.Down));
        }

        Assert.Equal("bomb", _page.ShownItemIds[_page.SelectedRow ?? 0]);

        _page.Update(Pressing(GameButton.Confirm));

        Assert.False(_page.IsChoosingTarget);
        Assert.Empty(_page.Sounds);
    }
}
