using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

public sealed class ObjectNamingTests
{
    [Fact]
    public void IdProblem_TakenId_SaysSo()
    {
        var naming = new ObjectNaming(["chest"], []) { Id = "chest" };

        Assert.Equal("The id chest is taken.", naming.IdProblem);
    }

    [Fact]
    public void IdProblem_ValidUnusedId_IsNull()
    {
        var naming = new ObjectNaming(["chest"], []) { Id = "tree_4" };

        Assert.Null(naming.IdProblem);
    }

    [Fact]
    public void CategoryProblem_Blank_SaysSo()
    {
        var naming = new ObjectNaming([], []) { Id = "tree_4", Category = " " };

        Assert.Equal("The category must not be empty.", naming.CategoryProblem);
    }

    [Fact]
    public void Categories_Duplicates_ListedOnceSorted()
    {
        var naming = new ObjectNaming([], ["House", "Chests", "House", "General"]);

        Assert.Equal(["Chests", "General", "House"], naming.Categories);
    }
}
