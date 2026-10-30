using Jrpg.Core.Party;

namespace Jrpg.Core.Exploration;

/// <summary>
/// A priced rest waiting for its answer (decision 382): the inn always charges, and once paid rests every member, even
/// one who needs nothing. Paying twice pays once; short of gold nothing changes; continuing the event without paying
/// leaves the party as it was.
/// </summary>
public sealed class InnStay
{
    private readonly PartyState _party;
    private bool _isPaid;

    internal InnStay(int price, PartyState party)
    {
        Price = price;
        _party = party;
    }

    /// <summary>The gold the stay costs.</summary>
    public int Price { get; }

    /// <summary>Whether the purse holds the price and the stay isn't paid yet.</summary>
    public bool CanPay => !_isPaid && _party.Purse.CanSpend(Price);

    /// <summary>Spends the price and rests every member.</summary>
    /// <returns>False, nothing changed, when the stay can't be paid.</returns>
    public bool TryPay()
    {
        if (_isPaid || !_party.Purse.TrySpend(Price))
        {
            return false;
        }

        _isPaid = true;
        _party.RestEveryone();

        return true;
    }
}
