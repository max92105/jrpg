namespace Jrpg.Core.Party;

/// <summary>
/// The party's gold, never below 0 nor above <see cref="MaxGold"/>. Gold past the cap is lost, whether it comes from a
/// battle or a sale: one overflow rule, so no trade is ever refused for a full purse.
/// </summary>
public sealed class Purse
{
    /// <summary>Seven digits fit every gold field the screens draw (the header, the rows, the picker).</summary>
    public const int MaxGold = 9_999_999;

    public int Gold { get; private set; }

    /// <returns>What was lost past <see cref="MaxGold"/>; 0 when it all fit.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is negative.</exception>
    public int Add(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        var room = MaxGold - Gold;
        var kept = Math.Min(amount, room);
        Gold += kept;
        return amount - kept;
    }

    /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is negative.</exception>
    public bool CanSpend(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return amount <= Gold;
    }

    /// <returns>False, with nothing taken, when the purse holds less than <paramref name="amount"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is negative.</exception>
    public bool TrySpend(int amount)
    {
        if (!CanSpend(amount))
        {
            return false;
        }

        Gold -= amount;
        return true;
    }
}
