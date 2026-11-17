using Jrpg.Core.Party;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.ClassFixtures;

namespace Jrpg.Editor.Tests.Party;

// The Classes page starts from the draft as it is now, and its whole session is one undo step (decision 945), none when
// nothing changed by content.
public sealed class ClassEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")) with
        {
            Classes = new Dictionary<string, ClassDefinition> { [Archer.Id] = Archer, [Scholar.Id] = Scholar },
        });

    private readonly ClassEditingViewModel _editing;

    public ClassEditingViewModelTests() => _editing = new ClassEditingViewModel(_draft);

    [Fact]
    public void Apply_EditedName_ChangesTheDraft()
    {
        var edited = _editing.Start();
        edited.Classes[0].Name = "Bowman";

        _editing.Apply(edited);

        Assert.Equal("Bowman", _draft.Current.Classes["archer"].Name);
    }

    [Fact]
    public void Apply_NothingChanged_RecordsNoStep()
    {
        _editing.Apply(_editing.Start());

        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void Start_AfterUndo_ReadsTheDraftAgain()
    {
        var edited = _editing.Start();
        edited.Classes[0].Name = "Bowman";
        _editing.Apply(edited);

        _draft.Undo();

        Assert.Equal("Archer", _editing.Start().Classes[0].Name);
    }
}
