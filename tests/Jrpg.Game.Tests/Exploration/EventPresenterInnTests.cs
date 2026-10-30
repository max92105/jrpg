using Jrpg.Core.Session;
using Jrpg.Game.Audio;
using Jrpg.Game.Exploration;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Exploration;

// A priced rest asks first, with its price (decisions 380, 382): Yes pays and plays the inn's beat, Yes without the
// gold shows the refusal, No continues the event unrested. A free rest asks nothing.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EventPresenterInnTests
{
    private const int Price = 20;
    private const int Damage = 10;

    [Fact]
    public void Update_PricedRest_AsksWithThePrice()
    {
        var (_, presenter) = StartAtTheInn();

        var question = presenter.Question!;
        Assert.Equal(RepositoryScreenText.Text.Messages.Text(MessageIds.InnQuestion), question.Question);
        Assert.Equal(Price, question.Price);
        Assert.Equal(0f, presenter.Blackness);
        Assert.False(presenter.IsShowingMessage);
    }

    [Fact]
    public void PromptYes_EnoughGold_FadesThenRests()
    {
        var (session, presenter) = StartAtTheInn();
        var gold = session.Party.Purse.Gold;
        presenter.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Up));

        presenter.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Confirm));
        presenter.Update(TimeSpan.FromSeconds(0.5), default);
        var blackness = presenter.Blackness;
        presenter.Update(TimeSpan.FromSeconds(2), default);

        Assert.Null(presenter.Question);
        Assert.Equal(1f, blackness);
        Assert.True(presenter.IsShowingMessage);
        Assert.Equal(gold - Price, session.Party.Purse.Gold);
        Assert.Equal(session.Party.Leader.Vitals.MaxHp, session.Party.Leader.Vitals.Hp);
    }

    [Fact]
    public void PromptYes_ShortOfGold_ShowsTheRefusalAndLeavesThePartyUnrested()
    {
        var (session, presenter) = StartAtTheInn();
        Assert.True(session.Party.Purse.TrySpend(session.Party.Purse.Gold - (Price - 1)));
        presenter.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Up));

        presenter.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Confirm));
        presenter.Update(TimeSpan.FromSeconds(0.5), default);

        Assert.Null(presenter.Question);
        Assert.True(presenter.IsShowingMessage);
        Assert.Equal(0f, presenter.Blackness);
        Assert.Equal(Price - 1, session.Party.Purse.Gold);
        Assert.Equal(session.Party.Leader.Vitals.MaxHp - Damage, session.Party.Leader.Vitals.Hp);
        presenter.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Confirm));
        Assert.False(session.Exploration.IsEventRunning);
    }

    [Fact]
    public void PromptNo_ContinuesWithoutResting()
    {
        var (session, presenter) = StartAtTheInn();
        var gold = session.Party.Purse.Gold;

        presenter.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Confirm));
        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.Null(presenter.Question);
        Assert.False(presenter.IsShowingMessage);
        Assert.False(session.Exploration.IsEventRunning);
        Assert.Equal(gold, session.Party.Purse.Gold);
        Assert.Equal(session.Party.Leader.Vitals.MaxHp - Damage, session.Party.Leader.Vitals.Hp);
    }

    // The Confirm that answers Yes came before the page exists, so it can't turn it; nor can one held through the fade.
    [Fact]
    public void PromptYes_TheAnsweringConfirm_DoesNotTurnTheRestPage()
    {
        var (session, presenter) = StartAtTheInn();
        presenter.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Up));

        presenter.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Confirm));
        presenter.Update(TimeSpan.FromSeconds(2), Pressing(GameButton.Confirm));

        Assert.True(presenter.IsShowingMessage);
        Assert.True(session.Exploration.IsEventRunning);
    }

    [Fact]
    public void Update_FreeRest_AsksNothing()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowRestingChest);
        var presenter = NewPresenter(session);
        presenter.Update(StartMapScenarios.OneUpdate, default);

        presenter.Update(TimeSpan.FromSeconds(0.5), default);

        Assert.Null(presenter.Question);
        Assert.Equal(1f, presenter.Blackness);
    }

    // The leader is hurt, so a rest shows; the first update puts the question up.
    private static (GameSession Session, EventPresenter Presenter) StartAtTheInn()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowInnChest);
        session.Party.Leader.Vitals.TakeDamage(Damage);
        var presenter = NewPresenter(session);
        presenter.Update(StartMapScenarios.OneUpdate, default);
        return (session, presenter);
    }

    private static EventPresenter NewPresenter(GameSession session)
    {
        session.Exploration.TryInteract();
        return new EventPresenter(session.Exploration, new EventLook(RepositoryScreenText.Text, new UiTextures()), new PlayerSettings(), new VoiceChannel());
    }
}
