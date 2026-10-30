using Jrpg.Core.Trade;

namespace Jrpg.Game.Trade;

/// <summary>
/// A count chosen between 1 and a most (decision 377): Up and Down by one, wrapping; the steps by ten, stopping at the
/// ends. Core's quote for the count is asked again when it changes, never per frame. Buying a stack and selling one
/// share it; the most is Core's, so the Game holds no count rule.
/// </summary>
internal sealed class QuantityPicker
{
    private const int Step = 10;
    private readonly int _most;
    private readonly Func<int, TradeQuote> _quote;

    /// <param name="most">The highest count, 1 or more: <c>Shop.MostBuyable</c>, or the owned count for a sale.</param>
    /// <param name="quote">Core's quote for a count.</param>
    public QuantityPicker(int most, Func<int, TradeQuote> quote)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(most, 1);
        _most = most;
        _quote = quote;
        Quote = quote(Count);
    }

    public int Count { get; private set; } = 1;

    /// <summary>Core's quote for <see cref="Count"/> copies.</summary>
    public TradeQuote Quote { get; private set; }

    public void Up() => Choose(Count == _most ? 1 : Count + 1);

    public void Down() => Choose(Count == 1 ? _most : Count - 1);

    public void StepUp() => Choose(Math.Min(_most, Count + Step));

    public void StepDown() => Choose(Math.Max(1, Count - Step));

    private void Choose(int count)
    {
        if (count == Count)
        {
            return;
        }

        Count = count;
        Quote = _quote(count);
    }
}
