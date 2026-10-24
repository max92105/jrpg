using Jrpg.Game.NewGame;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.NewGame;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class LeaderProfileViewTests
{
    private static readonly LeaderProfile _omer = new("Omer", "Omer.png", "Mage", "Casts spells.", "A scholar.", []);

    [Fact]
    public void Opacity_RightAfterShow_IsZero()
    {
        var view = new LeaderProfileView(RepositoryScreenText.Text, new UiTextures(), [_omer, _omer]);

        view.Show(1);

        Assert.Equal(0f, view.Opacity);
    }

    [Fact]
    public void Opacity_AfterProfileFade_IsOne()
    {
        var view = new LeaderProfileView(RepositoryScreenText.Text, new UiTextures(), [_omer, _omer]);
        view.Show(1);

        view.Update(TimeSpan.FromSeconds(0.2));

        Assert.Equal(1f, view.Opacity);
    }
}
