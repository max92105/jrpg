using Jrpg.Core.Stats;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class MemberVitalBarsTests
{
    private const int ValuePixels = 15;

    [Fact]
    public void Show_HurtMember_HpShowsItsHp()
    {
        var bars = new MemberVitalBars(RepositoryScreenText.Text, new UiTextures(), ValuePixels);
        var vitals = new Vitals(hp: 60, maxHp: 60);
        vitals.TakeDamage(15);

        bars.Show(vitals);

        Assert.Equal("45 / 60", bars.Hp.Value.Text);
    }
}
