using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Saving;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// Every page list glides its selection (decision 262), the System page's entries and the save files it opens included.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class SystemPageMotionTests
{
    [Fact]
    public void Advance_AfterMovingDown_SelectionReachesTheNextEntry()
    {
        var page = TestSystemPages.Create(NewSession(), new UiTextures());
        page.Open();
        page.Update(Pressing(GameButton.Down));
        Assert.Equal(0, page.SelectionPosition);

        page.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal(1, page.SelectionPosition);
    }

    [Fact]
    public void Advance_SaveFilesOpen_GlidesTheirSelection()
    {
        var slots = UnwrittenSaveSlots.Create();
        var text = RepositoryScreenText.Text;
        var ui = new UiTextures();
        var savePage = new SavePage(slots, NewSession(), text, ui);
        var page = new SystemPage(text, ui, savePage, new LoadPage(slots, text, ui));
        page.Open();
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Down));

        page.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal(1, savePage.SelectionPosition);
    }

    private static GameSession NewSession() => GameSession.StartNew(StartMapScenarios.BelowClosedDoor, "sarxian", new SeededRandom(1));
}
