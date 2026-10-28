using Jrpg.Core.Battle;
using Jrpg.Core.Loot;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;

namespace Jrpg.Game.Battle;

/// <summary>
/// What a won battle gave, over the frozen battle: each standing member's bar filling level by level, then what dropped
/// (decision 355). Confirm skips the filling; Confirm once it's done shows the drops, or leaves the battle when nothing
/// dropped; Confirm on the drops leaves. <see cref="VictoryScreenView"/> draws it.
/// </summary>
internal sealed class VictoryScreen
{
    private readonly ExperienceBarAnimation[] _bars;
    private bool _hasLeft;

    /// <param name="spoils">The won battle's experience, each standing member's gain, and the drops.</param>
    public VictoryScreen(BattleSpoils spoils)
    {
        Rewards = spoils.Rewards;
        Drops = spoils.Drops;
        _bars = [.. Rewards.Gains.Select(gain => new ExperienceBarAnimation(gain.Steps, gain.Member.Level))];
        Entrance = new VictoryEntrance(_bars.Length);
    }

    public VictoryRewards Rewards { get; }

    /// <summary>What dropped, already in the inventory.</summary>
    public Drops Drops { get; }

    /// <summary>Whether the Items step shows instead of the members' cards.</summary>
    public bool IsShowingDrops { get; private set; }

    /// <summary>How the title, the reward bar and the cards come in, before the bars fill.</summary>
    public VictoryEntrance Entrance { get; }

    /// <summary>One bar per gain, in the same order.</summary>
    public IReadOnlyList<ExperienceBarAnimation> Bars => _bars;

    /// <summary>Whether everything has come in and every bar has filled, so the footer shows "Next".</summary>
    public bool IsComplete => Entrance.IsComplete && Array.TrueForAll(_bars, bar => bar.IsComplete);

    /// <summary>
    /// Whether the card at <paramref name="index"/> shows the highest level (decision 148): only once its bar is full, so a
    /// member who reaches it in this battle first sees the last level fill.
    /// </summary>
    public bool IsAtMaxLevel(int index) => _bars[index].IsComplete && Rewards.Gains[index].IsAtMaxLevel;

    /// <returns>
    /// <see cref="BattleFinished"/> on the Confirm that leaves, once: after the bars are full when nothing dropped,
    /// or on the drops. Null otherwise.
    /// </returns>
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

        if (!IsShowingDrops && !Drops.IsEmpty)
        {
            IsShowingDrops = true;
            return null;
        }

        _hasLeft = true;
        return new BattleFinished();
    }
}
