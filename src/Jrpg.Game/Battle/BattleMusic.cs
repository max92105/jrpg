namespace Jrpg.Game.Battle;

/// <summary>
/// The music the battle screen calls for as its outcome shows: a victory's fanfare over silence and then the map's
/// track (decision 418), silence after a defeat, the map's track at once after an escape.
/// </summary>
internal sealed class BattleMusic
{
    /// <summary>The silence under the victory's fanfare: the placeholder fanfare's length.</summary>
    public static readonly TimeSpan FanfareHold = TimeSpan.FromSeconds(3.0);

    private TimeSpan _fanfareLeft;

    public BattleMusicCue Cue { get; private set; } = BattleMusicCue.Theme;

    public void ShowVictory()
    {
        Cue = BattleMusicCue.Silence;
        _fanfareLeft = FanfareHold;
    }

    public void ShowDefeat() => Cue = BattleMusicCue.Silence;

    public void ShowEscape() => Cue = BattleMusicCue.Field;

    public void Advance(TimeSpan delta)
    {
        if (_fanfareLeft <= TimeSpan.Zero)
        {
            return;
        }

        _fanfareLeft -= delta;
        if (_fanfareLeft <= TimeSpan.Zero)
        {
            Cue = BattleMusicCue.Field;
        }
    }
}
