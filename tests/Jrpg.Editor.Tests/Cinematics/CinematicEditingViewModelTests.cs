using Jrpg.Core.Cinematics;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Cinematics;

namespace Jrpg.Editor.Tests.Cinematics;

// The Cinematics page's session applied to the test world's draft as one undo step (ADR 0045, ADR 0046).
public sealed class CinematicEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));
    private readonly CinematicEditingViewModel _editing;

    public CinematicEditingViewModelTests() =>
        _editing = new CinematicEditingViewModel(_draft,
            () => new CinematicMediaChoices(() => [], () => [], () => [], _ => null));

    [Fact]
    public void Apply_AddedCinematic_IsOneUndoStep()
    {
        var catalog = _editing.Start();
        catalog.NewId = "intro";
        catalog.AddCinematicCommand.Execute(null);
        catalog.SelectedCinematic!.ImageToAdd = "intro/1.png";
        catalog.SelectedCinematic.AddStillCommand.Execute(null);

        _editing.Apply(catalog);

        Assert.Equal(["intro/1.png"], _draft.Current.Cinematics["intro"].Stills.Select(still => still.ImageFile));
        _draft.Undo();
        Assert.False(_draft.CanUndo);
    }

    // The page opened after a step reads the draft as it is then.
    [Fact]
    public void Start_AfterSetCinematics_ListsTheNewOne()
    {
        var still = new CinematicStill { ImageFile = "intro/1.png", Seconds = 4 };
        var intro = new CinematicDefinition { Id = "intro", Stills = [still] };
        _draft.SetCinematics([intro]);

        Assert.Equal(["intro"], _editing.Start().Cinematics.Select(cinematic => cinematic.Id));
    }

    [Fact]
    public void Apply_NothingChanged_RecordsNothing()
    {
        _editing.Apply(_editing.Start());

        Assert.False(_draft.CanUndo);
    }
}
