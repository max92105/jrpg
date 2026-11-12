using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The shop in the repository's content (the gold plan, N1): the shopkeeper, indoors since the start map's rework of
// 2026-11-06, opens the general store.
public sealed class RepositoryShopTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void InteriorMap_HasAnInteractEventOpeningTheGeneralStore()
    {
        var events = _catalog.Maps["proud_iron"].Layout.Objects.SelectMany(placed => placed.Events);

        Assert.Contains(events, mapEvent => mapEvent.Activation == EventActivation.Interact && mapEvent.Steps.Any(step => step is OpenShopStep { ShopId: "general_store" }));
    }
}
