using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Battle;

/// <summary>
/// An amount shown on its target when an effect lands (decisions 650-652), at full size at once and still: its digits
/// stepping down to the right, each hopping in turn, then gone at once. Pinned to the reference clip's frames #80-#84
/// (33 ms each): digit 0 is up at #80, digit 1 at #81, both still up at #82 and landed together at #83. A third digit,
/// which the two-digit clip can't show, keeps a frame at the top and lands after.
/// </summary>
/// <param name="Target">Who the effect landed on.</param>
/// <param name="Text">The amount, formatted once when the playback starts.</param>
/// <param name="Tone">What the number reports, which sets its color.</param>
/// <param name="ShowsAt">When it shows, from the impact: 0, or a later beat's start, such as a backfire.</param>
/// <param name="Stack">How many numbers showed on the same target at the same moment before it: each one sits
/// <see cref="StackSpacing"/> higher, so four hits read as four.</param>
/// <param name="Affinity">
/// What the target's resistance did to the hit, cued beside the number; neutral after a target's first number, so a
/// multi-hit says it once.
/// </param>
internal readonly record struct FloatingNumber(Combatant Target, string Text, FloatingNumberTone Tone, TimeSpan ShowsAt = default, int Stack = 0, HitAffinity Affinity = HitAffinity.Neutral)
{
    /// <summary>A little less than the 54 px digits, so a stack stays close; the outline keeps the numbers apart.</summary>
    public const float StackSpacing = 48;

    /// <summary>Between two digits, so they sit tight as the clip's.</summary>
    public const float DigitSpacing = -2;

    // Placeholder: a crit's number is drawn this much bigger than a normal one, so it stands out at a glance.
    private const float CriticalSize = 1.4f;

    // Each digit rests this much lower than the one before it, and hops this high.
    private const float DigitStep = 6;
    private const float HopHeight = 36;

    // A member's number sits just below their feet, as in the clip; an enemy's just above its name tag, never over it.
    private const int BelowTheFeet = 10;
    private const int AboveTheTag = 4;

    private static readonly TimeSpan _frame = TimeSpan.FromSeconds(1.0 / 30);
    private static readonly TimeSpan _firstHop = 2 * _frame;
    private static readonly TimeSpan _landing = 6 * _frame;
    private static readonly TimeSpan _life = TimeSpan.FromSeconds(0.57);

    /// <summary>White for damage, red-gold for a crit, green for healing, purple for Poison's tick.</summary>
    public static Color ColorOf(FloatingNumberTone tone) => tone switch
    {
        FloatingNumberTone.Damage => UiTheme.TextPrimary,
        FloatingNumberTone.Critical => UiTheme.FloatingCritical,
        FloatingNumberTone.Heal => UiTheme.FloatingHeal,
        FloatingNumberTone.Poison => UiTheme.FloatingPoison,
        _ => throw new ArgumentOutOfRangeException(nameof(tone), tone, null),
    };

    /// <summary>How much bigger than a normal number one of this tone is drawn: a crit's, else 1.</summary>
    public static float SizeOf(FloatingNumberTone tone) => tone == FloatingNumberTone.Critical ? CriticalSize : 1;

    /// <summary>
    /// Digit <paramref name="digit"/> rises over a frame from its turn (digit 0 two frames in), stays up at least a
    /// frame, and falls over a frame, landing at frame 6, or a frame after its own top for a third or later digit.
    /// </summary>
    /// <param name="digit">From 0, the leftmost.</param>
    /// <param name="sinceShown">Time since the number appeared.</param>
    /// <returns>Pixels below the row's rest line: the digit's step down, less its hop.</returns>
    public static float DigitOffsetAt(int digit, TimeSpan sinceShown)
    {
        var riseStart = _firstHop + (digit * _frame);
        var fallStart = Max(_landing - _frame, riseStart + (2 * _frame));
        var risen = Easing.ShareOf(sinceShown - riseStart, _frame);
        var fallen = Easing.ShareOf(sinceShown - fallStart, _frame);
        return (digit * DigitStep) - (HopHeight * (risen - fallen));
    }

    /// <param name="sinceShown">Time since the number's moment; negative before it.</param>
    public static bool IsShownAt(TimeSpan sinceShown) => sinceShown >= TimeSpan.Zero && sinceShown < _life;

    /// <param name="spot">Where the target stands.</param>
    /// <param name="tag">An enemy's name tag; null for a party member.</param>
    /// <param name="digits">
    /// How many digits the number has: above a tag, its last, lowest digit keeps clear of it.
    /// </param>
    /// <returns>Where the number's row rests, its first digit's bottom, on the screen.</returns>
    public static float RowBottomOf(Rectangle spot, Rectangle? tag, int digits) =>
        tag is { } area ? area.Top - AboveTheTag - ((digits - 1) * DigitStep) : spot.Bottom + BelowTheFeet;

    private static TimeSpan Max(TimeSpan first, TimeSpan second) => first > second ? first : second;
}
