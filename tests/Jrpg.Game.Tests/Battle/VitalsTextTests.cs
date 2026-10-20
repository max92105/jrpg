using Jrpg.Core.Stats;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class VitalsTextTests
{
    private readonly VitalsText _text = new(RepositoryScreenText.Text.Messages);

    [Fact]
    public void Refresh_SameValues_KeepsSameStrings()
    {
        var vitals = new Vitals(150, 200, 30, 40);
        _text.Refresh(vitals);
        var (hp, maxHp, mp, maxMp) = (_text.Hp, _text.MaxHp, _text.Mp, _text.MaxMp);

        _text.Refresh(vitals);

        Assert.Same(hp, _text.Hp);
        Assert.Same(maxHp, _text.MaxHp);
        Assert.Same(mp, _text.Mp);
        Assert.Same(maxMp, _text.MaxMp);
    }

    [Fact]
    public void Refresh_HpChanged_ReformatsHp()
    {
        var vitals = new Vitals(150, 200, 30, 40);
        _text.Refresh(vitals);
        var maxHp = _text.MaxHp;
        vitals.TakeDamage(25);

        _text.Refresh(vitals);

        Assert.Equal(("125", "/200"), (_text.Hp, _text.MaxHp));
        Assert.Same(maxHp, _text.MaxHp);
    }

    [Fact]
    public void Refresh_MpChanged_ReformatsMpOnly()
    {
        var vitals = new Vitals(150, 200, 30, 40);
        _text.Refresh(vitals);
        var (hp, maxMp) = (_text.Hp, _text.MaxMp);
        vitals.TrySpendMp(8);

        _text.Refresh(vitals);

        Assert.Equal("22", _text.Mp);
        Assert.Same(hp, _text.Hp);
        Assert.Same(maxMp, _text.MaxMp);
    }

    [Fact]
    public void Refresh_FallenMember_HpIsZero()
    {
        var vitals = new Vitals(10, 200, 30, 40);
        vitals.TakeDamage(50);

        _text.Refresh(vitals);

        Assert.Equal("0", _text.Hp);
    }
}
