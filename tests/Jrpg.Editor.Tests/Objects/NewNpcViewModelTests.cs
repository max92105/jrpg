using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

/// <summary>The New NPC dialog over set-up sheets (ADR 0043), and the same dialog changing an NPC's sprite.</summary>
public sealed class NewNpcViewModelTests
{
    private static readonly IReadOnlyList<FieldSpriteSheet> _sheets =
        [Sheet("player.png"), Sheet("guard.png"), Sheet("npc_placeholder.png")];

    [Fact]
    public void SpriteFiles_Records_PlaceholderFirstThenByName() =>
        Assert.Equal(["npc_placeholder.png", "guard.png", "player.png"], new NewNpcViewModel([], _sheets).SpriteFiles);

    [Fact]
    public void SpriteFile_PlaceholderListed_IsChosenByDefault() =>
        Assert.Equal("npc_placeholder.png", new NewNpcViewModel([], _sheets).SpriteFile);

    [Fact]
    public void SpriteProblem_NoRecord_PointsToCharacterSprites()
    {
        var newNpc = new NewNpcViewModel([], []);

        Assert.Equal("Set up a character sprite first: NPCs > Character sprites", newNpc.SpriteProblem);
    }

    // Only set-up files are listed, but the sprite is a string to the binding: one without a record is refused too.
    [Fact]
    public void SpriteProblem_FileWithoutRecord_SaysSo()
    {
        var newNpc = new NewNpcViewModel([], _sheets) { SpriteFile = "missing.png" };

        Assert.Equal("missing.png has no record; set it up in NPCs > Character sprites", newNpc.SpriteProblem);
    }

    [Fact]
    public void IdProblem_IdTaken_SaysSo()
    {
        var newNpc = new NewNpcViewModel(["villager"], _sheets) { Id = "villager" };

        Assert.NotNull(newNpc.IdProblem);
    }

    [Fact]
    public void ToDefinition_AnyId_IsOneCellNpcWithLowerHalfOutline()
    {
        var newNpc = new NewNpcViewModel([], _sheets) { Id = "guard", SpriteFile = "guard.png" };

        var definition = newNpc.ToDefinition();

        Assert.Equal(("guard", 1, 1, "NPC", "guard.png"), (definition.Id, definition.Width, definition.Height, definition.Category, definition.FieldSpriteFile));
        Assert.Equal([new Point2D(0.2, 0.5), new Point2D(0.8, 0.5), new Point2D(0.8, 1), new Point2D(0.2, 1)], definition.CollisionPolygon);
        Assert.Empty(definition.Tiles);
        Assert.Empty(definition.AbovePlayerZones);
    }

    [Fact]
    public void ForChange_Npc_FixesTheIdAndPicksItsSprite()
    {
        var dialog = NewNpcViewModel.ForChange(Villager(), _sheets);

        Assert.True(dialog.IsIdFixed);
        Assert.Equal(("villager", "npc_placeholder.png"), (dialog.Id, dialog.SpriteFile));
        Assert.Null(dialog.IdProblem);
    }

    // The sprite is the one field changed (ADR 0015): the outline drawn by hand and the category stay.
    [Fact]
    public void ToDefinition_ChangeMode_KeepsOutlineAndCategory()
    {
        var villager = Villager();
        var dialog = NewNpcViewModel.ForChange(villager, _sheets);

        dialog.SpriteFile = "guard.png";

        Assert.Equal(villager with { FieldSpriteFile = "guard.png" }, dialog.ToDefinition());
    }

    private static ObjectDefinition Villager() => new()
    {
        Id = "villager",
        Width = 1,
        Height = 1,
        CollisionPolygon = [new Point2D(0, 0.7), new Point2D(1, 0.7), new Point2D(1, 1), new Point2D(0, 1)],
        Category = "Townsfolk",
        Tiles = [],
        FieldSpriteFile = "npc_placeholder.png",
        AbovePlayerZones = [],
    };

    private static FieldSpriteSheet Sheet(string file) => new()
    {
        File = file,
        FrameWidth = 48,
        FrameHeight = 48,
        Scale = 1,
        Walk = new FacingCycle
        {
            Rows = new FacingRows { Down = 0, Left = 1, Right = 2, Up = 3 },
            Frames = new TileAnimation { FrameCount = 8, FrameMilliseconds = 100 },
        },
    };
}
