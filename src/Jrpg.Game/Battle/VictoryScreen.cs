using Jrpg.Core.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;

namespace Jrpg.Game.Battle;

/// <summary>
/// What a won battle gave, over the frozen battle (legacy <c>VictoryScreen</c>): each standing member's bar filling level
/// by level. Confirm skips the filling; Confirm once it's done leaves the battle. <see cref="VictoryScreenView"/> draws it.
/// </summary>
internal sealed class VictoryScreen
{
    private readonly ExperienceBarAnimation[] _bars;
    private bool _hasLeft;

    /// <param name="rewards">The won battle's experience and each standing member's gain.</param>
    public VictoryScreen(VictoryRewards rewards)
    {
        Rewards = rewards;
        _bars = [.. rewards.Gains.Select(gain => new ExperienceBarAnimation(gain.Steps, gain.Member.Level))];
        Entrance = new VictoryEntrance(_bars.Length);
    }

    public VictoryRewards Rewards { get; }

    /// <summary>How the title, the reward bar and the cards come in, before the bars fill.</summary>
    public VictoryEntrance Entrance { get; }

    /// <summary>One bar per gain, in the same order.</summary>
    public IReadOnlyList<ExperienceBarAnimation> Bars => _bars;

    /// <summary>Whether everything has come in and every bar has filled, so the footer shows "Next".</summary>
    public bool IsComplete => Entrance.IsComplete && Array.TrueForAll(_bars, bar => bar.IsComplete);

    /// <returns><see cref="BattleFinished"/> on the Confirm after the bars are full, once; null otherwise.</returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        if (_hasLeft)
        {
            return null;
        }

        // The bars fill only once the entrance is over, so they get only the update's time past it.
        var barTimeBefore = Entrance.BarTime;
        Entrance.Update(delta);
        var barDelta = Entrance.BarTime - barTimeBefore;
        foreach (var bar in _bars)
        {
            bar.Update(barDelta);
        }

        if (!input.IsPressed(GameButton.Confirm))
        {
            return null;
        }

        if (!IsComplete)
        {
            Entrance.Skip();
            Array.ForEach(_bars, bar => bar.Skip());
            return null;
        }

        _hasLeft = true;
        return new BattleFinished();
    }
}
