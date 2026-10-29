using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The forge in the repository's content (the forge plan, N1): someone on the start map opens it.
public sealed class RepositoryForgeTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void StartMap_HasAnInteractEventOpeningTheForge()
    {
        var events = _catalog.Maps["start"].Layout.Objects.SelectMany(placed => placed.Events);

        Assert.Contains(events, mapEvent => mapEvent.Activation == EventActivation.Interact && mapEvent.Steps.Any(step => step is OpenForgeStep));
    }
}
