using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// How far the menu's tabs have folded into their icon rail while a page is open (decision 286): from 0, the full tabs,
/// to 1, only the rail of each on screen. It eases out toward wherever it is heading, and turning back mid-way starts
/// from where the tabs are, so they never jump.
/// </summary>
internal sealed class TabFold
{
    /// <summary>How long a full fold or unfold takes (placeholder, as the tab slide-in and page transition).</summary>
    public static readonly TimeSpan Duration = TimeSpan.FromSeconds(0.2);

    /// <summary>How much of each tab stays on screen once folded: its point and its icon (placeholder).</summary>
    public const int RailWidth = 120;

    // The raw share of the way toward the current target, 0 to 1; Share eases it in the direction of travel.
    private float _progress = 1;
    private bool _isFolding;

    /// <summary>0 with the tabs unfolded, 1 folded into the rail.</summary>
    public float Share => _isFolding ? Easing.CubicOut(_progress) : 1 - Easing.CubicOut(_progress);

    /// <summary>How far each tab is pushed off the screen's left edge, in virtual pixels.</summary>
    public int HiddenWidth => (int)Math.Round(Share * (NavTabShape.Width - RailWidth));

    public void Fold()
    {
        if (_isFolding)
        {
            return;
        }

        var share = Share;
        _isFolding = true;
        _progress = ProgressToward(1 - share);
    }

    public void Unfold()
    {
        if (!_isFolding)
        {
            return;
        }

        var share = Share;
        _isFolding = false;
        _progress = ProgressToward(share);
    }

    public void Advance(TimeSpan delta) => _progress = Math.Min(1, _progress + (float)(delta / Duration));

    // The progress at which the eased share has left `remaining` of the way to go: CubicOut's inverse, 1 - (1 - p)³ =
    // 1 - remaining.
    private static float ProgressToward(float remaining) => 1 - MathF.Cbrt(remaining);
}
