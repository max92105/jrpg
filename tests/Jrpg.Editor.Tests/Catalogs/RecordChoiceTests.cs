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

    [Fact]
    public void Of_Name_ChoosesTheRecord()
    {
        Assert.Equal(new RecordChoice("fireball", "Fireball"), RecordChoice.Of("fireball", "Fireball"));
    }

    [Fact]
    public void Of_NoName_IsMissing()
    {
        Assert.Equal(RecordChoice.Missing("fireball"), RecordChoice.Of("fireball", null));
    }

    // Held against both orders: the ids sort Zap first, the names Blaze first, and the two Blazes by id.
    [Fact]
    public void ByName_Choices_OrdersByNameThenId()
    {
        RecordChoice[] choices = [new("bolt", "Zap"), new("fire2", "Blaze"), new("fire1", "Blaze")];

        var ordered = RecordChoice.ByName(choices);

        Assert.Equal(["fire1", "fire2", "bolt"], ordered.Select(choice => choice.Id));
    }
}
