using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// The comparison's band says what it lists (E2): what the highlighted worn item gives, or what a choice would change.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EquipmentPageCaptionTests
{
    private readonly GameSession _session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    [Fact]
    public void Open_NoChoice_CaptionSaysGives()
    {
        var page = NewPage();

        Assert.Equal(MessageIds.EquipmentGives, page.ChangesCaptionId);
    }

    [Fact]
    public void Update_ChoiceOpens_CaptionSaysChanges()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.Confirm));

        Assert.True(page.IsChoosing);
        Assert.Equal(MessageIds.EquipmentChanges, page.ChangesCaptionId);
    }

    [Fact]
    public void Update_ChoiceCloses_CaptionSaysGives()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Confirm));

        page.Update(Pressing(GameButton.Cancel));

        Assert.False(page.IsChoosing);
        Assert.Equal(MessageIds.EquipmentGives, page.ChangesCaptionId);
    }

    private EquipmentPage NewPage() => new(RepositoryScreenText.Text, new UiTextures(), _session, StartMapScenarios.BelowClosedDoor);
}
