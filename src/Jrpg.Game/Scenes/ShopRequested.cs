namespace Jrpg.Game.Scenes;

/// <summary>A map event reached a shop step: that shop opens over the map (decision 353).</summary>
/// <param name="ShopId">The shop in the catalog.</param>
internal sealed record ShopRequested(string ShopId) : SceneRequest;
