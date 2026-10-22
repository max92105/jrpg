using Jrpg.Core.Geometry;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

public sealed class NewNpcViewModelTests
{
    private static readonly IReadOnlyList<string> _sprites = ["guard.png", "npc_placeholder.png", "player.png"];

    [Fact]
    public void SpriteFile_PlaceholderListed_IsChosenByDefault()
    {
        Assert.Equal("npc_placeholder.png", new NewNpcViewModel([], _sprites).SpriteFile);
    }

    [Fact]
    public void SpriteFile_NoSpriteListed_HasAProblem()
    {
        Assert.NotNull(new NewNpcViewModel([], []).SpriteProblem);
    }

    [Fact]
    public void IdProblem_IdTaken_SaysSo()
    {
        var newNpc = new NewNpcViewModel(["villager"], _sprites) { Id = "villager" };

        Assert.NotNull(newNpc.IdProblem);
    }

    [Fact]
    public void ToDefinition_AnyId_IsOneCellNpcWithLowerHalfOutline()
    {
        var newNpc = new NewNpcViewModel([], _sprites) { Id = "guard", SpriteFile = "guard.png" };

        var definition = newNpc.ToDefinition();

        Assert.Equal(("guard", 1, 1, "NPC", "guard.png"), (definition.Id, definition.Width, definition.Height, definition.Category, definition.FieldSpriteFile));
        Assert.Equal([new Point2D(0.2, 0.5), new Point2D(0.8, 0.5), new Point2D(0.8, 1), new Point2D(0.2, 1)], definition.CollisionPolygon);
        Assert.Empty(definition.Tiles);
        Assert.Empty(definition.AbovePlayerZones);
    }
}
