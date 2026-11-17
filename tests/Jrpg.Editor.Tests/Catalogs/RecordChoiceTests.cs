using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// One record as a box offers it (decision 967): by name with its id, or marked missing when the draft lacks the id.
public sealed class RecordChoiceTests
{
    [Fact]
    public void Label_NameAndId_ShowsBoth() =>
        Assert.Equal("Fireball (fireball)", new RecordChoice("fireball", "Fireball").Label);

    [Fact]
    public void Missing_Id_SaysMissing()
    {
        var missing = RecordChoice.Missing("fireball");

        Assert.Equal(("fireball", true, "(missing) fireball"), (missing.Id, missing.IsMissing, missing.Label));
    }
}
