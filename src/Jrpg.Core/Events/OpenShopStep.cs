namespace Jrpg.Core.Events;

/// <summary>
/// Opens a shop, a shopkeeper's screen (ADR 0031, decision 353), and waits until the player closes it.
/// </summary>
/// <param name="ShopId">The shop in <c>shops.json</c>.</param>
public sealed record OpenShopStep(string ShopId) : EventStep;
