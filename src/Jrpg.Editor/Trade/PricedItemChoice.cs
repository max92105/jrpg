namespace Jrpg.Editor.Trade;

/// <summary>
/// One item the Shops window's picker offers (decision 584): a priced one, shown by name with the id beside it and the
/// price, so two items named alike are told apart.
/// </summary>
internal sealed record PricedItemChoice(string Id, string Name, int Price)
{
    public string Label => $"{Name} ({Id}), {Price} g";
}
