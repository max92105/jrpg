using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

public sealed class ObjectIdsTests
{
    [Fact]
    public void ProblemOf_IdTaken_SaysSo()
    {
        Assert.Equal("The id tree is taken.", ObjectIds.ProblemOf("tree", ["chest", "tree"]));
    }

    [Theory]
    [InlineData("Tree 4")]
    [InlineData("tree-4")]
    public void ProblemOf_CharactersOutsideSnakeCase_SaysWhichAreAllowed(string id)
    {
        Assert.Equal("The id may only use lowercase letters, digits and _ (like tree_4).", ObjectIds.ProblemOf(id, []));
    }

    [Fact]
    public void ProblemOf_Empty_SaysSo()
    {
        Assert.Equal("The id must not be empty.", ObjectIds.ProblemOf(" ", []));
    }

    [Fact]
    public void ProblemOf_SnakeCaseId_IsNull()
    {
        Assert.Null(ObjectIds.ProblemOf("tree_4", ["tree"]));
    }
}
