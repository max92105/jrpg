using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>"Level Up!" popping in on a victory card: drawn 1.5x large, settling to its size.</summary>
internal static class LevelUpPop
{
    private const float StartScale = 1.5f;
    private static readonly TimeSpan _length = TimeSpan.FromSeconds(0.35);

    /// <param name="sinceLevelUp">Time since the bar filled the level.</param>
    /// <returns>The text's scale: 1.5 at the level-up, easing out to 1.</returns>
    public static float ScaleAt(TimeSpan sinceLevelUp)
    {
        var eased = Easing.CubicOut(Easing.ShareOf(sinceLevelUp, _length));
        return StartScale + ((1 - StartScale) * eased);
    }
}
