using Jrpg.Editor.Party;

namespace Jrpg.Editor.Tests.Party;

// What a rule's editable id combo lists: the class's other ids, and the typed id when it isn't one of them.
public sealed class ResourceIdChoicesTests
{
    private static readonly string[] _classmateIds = ["mana", "fury"];

    // The same list, so the combo keeps it as the id is typed.
    [Fact]
    public void Of_TypedIdListed_IsTheOfferedList() => Assert.Same(_classmateIds, ResourceIdChoices.Of(_classmateIds, "fury"));

    [Fact]
    public void Of_EmptyTypedId_IsTheOfferedList() => Assert.Same(_classmateIds, ResourceIdChoices.Of(_classmateIds, ""));

    [Fact]
    public void Of_TypedIdNotListed_AddsItLast() =>
        Assert.Equal(["mana", "fury", "calm"], ResourceIdChoices.Of(_classmateIds, "calm"));
}
