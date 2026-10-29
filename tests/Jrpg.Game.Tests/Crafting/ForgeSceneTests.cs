using Jrpg.Game.Crafting;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Crafting;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class ForgeSceneTests
{
    // A second close request would make the flow pop what lies under the Forge.
    [Fact]
    public void Update_Cancel_RequestsCloseOnce()
    {
        var forge = new ForgeScene(RepositoryScreenText.Text);

        var first = forge.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));
        var second = forge.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));

        Assert.IsType<ForgeClosed>(first);
        Assert.Null(second);
    }

    [Fact]
    public void Update_Confirm_RequestsNothing()
    {
        var forge = new ForgeScene(RepositoryScreenText.Text);

        Assert.Null(forge.Update(TimeSpan.Zero, Pressing(GameButton.Confirm)));
    }
}
