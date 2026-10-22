using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

public sealed class NewObjectViewModelTests
{
    private static readonly SheetCellRectangle _cells = new("forest", 0, 0, 3, 4);

    [Fact]
    public void CategoryProblem_Empty_SaysSo()
    {
        var newObject = new NewObjectViewModel([], [], _cells) { Id = "tree_4", Category = " " };

        Assert.Equal("The category must not be empty.", newObject.CategoryProblem);
        Assert.Null(newObject.IdProblem);
        Assert.Equal("3 x 4 cells", newObject.SizeText);
    }

    [Fact]
    public void Categories_RepositoryObjects_AreDistinctAndSorted()
    {
        var objects = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")).Objects.Values;

        var newObject = new NewObjectViewModel([.. objects.Select(definition => definition.Id)], [.. objects.Select(definition => definition.Category)], _cells);

        Assert.Equal(["Chests", "General", "House"], newObject.Categories);
    }
}
