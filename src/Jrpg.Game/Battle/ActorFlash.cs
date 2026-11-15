namespace Jrpg.Game.Battle;

/// <summary>
/// The acting battler's double blink as its action winds up, instead of a step (decision 648): a short blink, then a
/// longer one fading at its end, and nothing before, between or after. The times are read off the developer's reference
/// clip, sampled every 33 ms.
/// </summary>
internal static class ActorFlash
{
    private static readonly TimeSpan _firstBlinkStart = TimeSpan.FromSeconds(0.10);
    private static readonly TimeSpan _firstBlinkLength = TimeSpan.FromMilliseconds(33);
    private static readonly TimeSpan _secondBlinkStart = TimeSpan.FromSeconds(0.20);
    private static readonly TimeSpan _secondBlinkLength = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan _secondBlinkFade = TimeSpan.FromMilliseconds(30);

    /// <returns>From 0, not blinking, to 1, a full blink.</returns>
    public static float StrengthAt(TimeSpan sinceWindUpStart)
    {
        if (IsWithin(sinceWindUpStart, _firstBlinkStart, _firstBlinkLength))
        {
            return 1;
        }

        if (!IsWithin(sinceWindUpStart, _secondBlinkStart, _secondBlinkLength))
        {
            return 0;
        }

        var untilEnd = _secondBlinkStart + _secondBlinkLength - sinceWindUpStart;
        return untilEnd >= _secondBlinkFade ? 1 : (float)(untilEnd / _secondBlinkFade);
    }

    private static bool IsWithin(TimeSpan time, TimeSpan start, TimeSpan length) => time >= start && time < start + length;
}
