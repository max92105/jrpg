using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Battle;

/// <summary>
/// An amount that pops over its target when an effect lands: it grows from 60 % to full size in 0.18 s, rises 50 px
/// over 1.6 s, easing out, and fades over the second half of that.
/// </summary>
/// <param name="Target">Who the effect landed on.</param>
/// <param name="Text">The amount, formatted once when the playback starts.</param>
/// <param name="Tone">What the number reports, which sets its color.</param>
/// <param name="ShowsAt">When it pops, from the impact: 0, or a later beat's start, such as a backfire.</param>
/// <param name="Stack">How many numbers popped on the same target at the same moment before it: each one sits
/// <see cref="StackSpacing"/> higher, so four hits read as four.</param>
/// <param name="Affinity">
/// What the target's resistance did to the hit, cued beside the number; neutral after a target's first number, so a
/// multi-hit says it once.
/// </param>
internal readonly record struct FloatingNumber(Combatant Target, string Text, FloatingNumberTone Tone, TimeSpan ShowsAt = default, int Stack = 0, HitAffinity Affinity = HitAffinity.Neutral)
{
    public const float RiseDistance = 50;

    /// <summary>Less than the 48 px text, so a stack stays close; the outline keeps the numbers apart.</summary>
    public const float StackSpacing = 36;

    private const float PopStartScale = 0.6f;
    private static readonly TimeSpan _pop = TimeSpan.FromSeconds(0.18);
    private static readonly TimeSpan _life = TimeSpan.FromSeconds(1.6);

    /// <summary>White for damage, green for healing, purple for Poison's tick.</summary>
    public static Color ColorOf(FloatingNumberTone tone) => tone switch
    {
        FloatingNumberTone.Damage => UiTheme.TextPrimary,
        FloatingNumberTone.Heal => UiTheme.FloatingHeal,
        FloatingNumberTone.Poison => UiTheme.FloatingPoison,
        _ => throw new ArgumentOutOfRangeException(nameof(tone), tone, null),
    };

    /// <param name="sinceShown">Time since the number appeared.</param>
    public static FloatingNumberLook At(TimeSpan sinceShown)
    {
        var popShare = Easing.ShareOf(sinceShown, _pop);
        var lifeShare = Easing.ShareOf(sinceShown, _life);
        var easedRise = 1 - ((1 - lifeShare) * (1 - lifeShare));
        var opacity = lifeShare < 0.5f ? 1 : 2 * (1 - lifeShare);
        return new FloatingNumberLook(PopStartScale + ((1 - PopStartScale) * popShare), RiseDistance * easedRise, opacity);
    }
}
