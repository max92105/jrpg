using Jrpg.Core.Cinematics;
using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Events;

// The cinematics a cinematic step offers (ADR 0046), read from the draft when the dialog opens, on the test world's
// start map whose chest holds an event.
public sealed class EventEditingViewModelCinematicTests
{
    private static readonly ContentCatalog _testWorld =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    [Fact]
    public void StartEditingAt_DraftCinematics_OfferedSorted()
    {
        var catalog = _testWorld with
        {
            Cinematics = new Dictionary<string, CinematicDefinition>
            {
                ["zeta"] = Cinematic("zeta"),
                ["alpha"] = Cinematic("alpha"),
            },
        };
        var eventEditing = new EventEditingViewModel(new CatalogDraft(catalog), new OpenMaps(), new LayerChoiceViewModel())
        {
            OpenMap = new MapViewModel(catalog.Maps["start"]),
        };

        var events = eventEditing.StartEditingAt(ChestCenter(catalog))!;

        Assert.Equal(["alpha", "zeta"], events.Choices.CinematicIds);
    }

    private static CinematicDefinition Cinematic(string id) =>
        new() { Id = id, Stills = [new CinematicStill { ImageFile = $"{id}/1.png", Seconds = 4 }] };

    private static Point2D ChestCenter(ContentCatalog catalog)
    {
        var chest = catalog.Maps["start"].Layout.Objects.Single(placed => placed.Uid == "0bff6243");
        return new Point2D(chest.Position.X + 0.5, chest.Position.Y + 0.5);
    }
}
