using Jrpg.Core.Battle;
using Jrpg.Core.Loot;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// What a won battle gave, over the frozen battle: each standing member's bar filling level by level, then what dropped
/// (decision 355). Confirm skips the filling; Confirm once it's done shows the drops, or leaves the battle when nothing
/// dropped; Confirm on the drops leaves. A level filled sounds as its "Level Up!" shows, by the bar or the skip; the
/// drops showing sound the field's found item, and leaving the confirm (decisions 430, 432).
/// <see cref="VictoryScreenView"/> draws it.
/// </summary>
internal sealed class VictoryScreen
{
    private readonly ExperienceBarAnimation[] _bars;
    private readonly AskedSounds _sounds = new();
    private bool _hasLeft;

    /// <param name="spoils">The won battle's experience, each standing member's gain, the drops and the gold.</param>
    public VictoryScreen(BattleSpoils spoils)
    {
        Rewards = spoils.Rewards;
        Drops = spoils.Drops;
        Gold = spoils.Gold;
        _bars = [.. Rewards.Gains.Select(gain => new ExperienceBarAnimation(gain.Steps, gain.Member.Level))];
        Entrance = new VictoryEntrance(_bars.Length);
    }

    public VictoryRewards Rewards { get; }

    /// <summary>What dropped, already in the inventory.</summary>
    public Drops Drops { get; }

    /// <summary>What the purse took (ADR 0031); 0 shows no gold row.</summary>
    public int Gold { get; }

    /// <summary>Whether the Items step shows instead of the members' cards.</summary>
    public bool IsShowingDrops { get; private set; }

    /// <summary>How the title, the reward bar and the cards come in, before the bars fill.</summary>
    public VictoryEntrance Entrance { get; }

    /// <summary>The effects the last update asked for.</summary>
    public IReadOnlyList<string> Sounds => _sounds.Files;

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
        _sounds.Clear();
        if (_hasLeft)
        {
            return null;
        }

        // The bars fill only once the entrance is over, so they get only the update's time past it.
        var barTimeBefore = Entrance.BarTime;
        Entrance.Update(delta);
        var barDelta = Entrance.BarTime - barTimeBefore;
        var levelUpsBeforeFill = LevelUps();
        foreach (var bar in _bars)
        {
            bar.Update(barDelta);
        }

        AskLevelUpIfAny(levelUpsBeforeFill);

        if (!input.IsPressed(GameButton.Confirm))
        {
            return null;
        }

        if (!IsComplete)
        {
            var levelUpsBeforeSkip = LevelUps();
            Entrance.Skip();
            Array.ForEach(_bars, bar => bar.Skip());
            AskLevelUpIfAny(levelUpsBeforeSkip);
            return null;
        }

        if (!IsShowingDrops && !Drops.IsEmpty)
        {
            IsShowingDrops = true;
            _sounds.Ask(UiSoundFiles.Field.ItemGot);
            return null;
        }

        _hasLeft = true;
        _sounds.Ask(UiSoundFiles.Effects.MenuConfirm);
        return new BattleFinished();
    }

    // Two members levelling in one update ask it once.
    private void AskLevelUpIfAny(int levelUpsBefore)
    {
        if (LevelUps() > levelUpsBefore)
        {
            _sounds.Ask(UiSoundFiles.Battle.LevelUp);
        }
    }

    private int LevelUps()
    {
        var levelUps = 0;
        foreach (var bar in _bars)
        {
            levelUps += bar.LevelUps;
        }

        return levelUps;
    }
}
