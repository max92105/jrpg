using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// An art page's problem line: the chosen row's own first, then another's named by file.
public sealed class ArtImageRowProblemTests
{
    private static readonly Row _fine = new("A.png", null);
    private static readonly Row _late = new("B.png", "Too late.");
    private static readonly Row _early = new("C.png", "Too early.");

    [Fact]
    public void FirstOf_ChosenRowHasOne_IsItsOwnBare()
    {
        var problem = ArtImageRowProblem.FirstOf(_early, [_fine, _late, _early], row => row.File, row => row.Problem);

        Assert.Equal("Too early.", problem);
    }

    [Fact]
    public void FirstOf_ChosenRowFine_IsTheFirstOtherNamedByFile()
    {
        var problem = ArtImageRowProblem.FirstOf(_fine, [_fine, _late, _early], row => row.File, row => row.Problem);

        Assert.Equal("B.png: Too late.", problem);
    }

    [Fact]
    public void FirstOf_NoneChosen_IsTheFirstNamedByFile()
    {
        var problem = ArtImageRowProblem.FirstOf(null, [_early, _late], row => row.File, row => row.Problem);

        Assert.Equal("C.png: Too early.", problem);
    }

    [Fact]
    public void FirstOf_NoRowHasOne_IsNull() =>
        Assert.Null(ArtImageRowProblem.FirstOf(_fine, [_fine], row => row.File, row => row.Problem));

    private sealed record Row(string File, string? Problem);
}
