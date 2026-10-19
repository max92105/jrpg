namespace Jrpg.Game.Dialogue;

/// <summary>
/// How much of a page is shown as it types out, one character per <c>characterTime</c> (developer decision 54).
/// Time left over carries to the next update, so the speed doesn't depend on the frame rate.
/// </summary>
/// <param name="length">The page's length in characters.</param>
/// <param name="characterTime">Zero shows the whole page at once.</param>
internal sealed class Typewriter(int length, TimeSpan characterTime)
{
    private TimeSpan _elapsed;

    public int VisibleLength { get; private set; } = characterTime <= TimeSpan.Zero ? length : 0;

    public bool IsComplete => VisibleLength == length;

    public void Update(TimeSpan delta)
    {
        if (IsComplete)
        {
            return;
        }

        _elapsed += delta;
        VisibleLength = (int)Math.Min(length, _elapsed.Ticks / characterTime.Ticks);
    }

    public void RevealAll() => VisibleLength = length;
}
