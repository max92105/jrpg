using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Objects;

/// <summary>The New NPC dialog (ADR 0015): the id typed and the field sprite chosen for a new NPC object.</summary>
internal sealed partial class NewNpcViewModel : ObservableObject
{
    /// <summary>The sprite every NPC shares until real art exists (decision 69).</summary>
    public const string PlaceholderSprite = "npc_placeholder.png";

    private const string NpcCategory = "NPC";

    // Decision 129: the lower half of the cell blocks, so the player walks behind an NPC's head.
    private static readonly IReadOnlyList<Point2D> _lowerHalfOutline =
        [new Point2D(0.2, 0.5), new Point2D(0.8, 0.5), new Point2D(0.8, 1), new Point2D(0.2, 1)];

    private readonly IReadOnlyCollection<string> _takenIds;

    /// <param name="takenIds">The object ids already in the catalog.</param>
    /// <param name="spriteFiles">The PNG names in the field-sprite folder.</param>
    public NewNpcViewModel(IReadOnlyCollection<string> takenIds, IReadOnlyList<string> spriteFiles)
    {
        _takenIds = takenIds;
        SpriteFiles = spriteFiles;
        SpriteFile = spriteFiles.Contains(PlaceholderSprite) ? PlaceholderSprite : spriteFiles.Count > 0 ? spriteFiles[0] : "";
    }

    public IReadOnlyList<string> SpriteFiles { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IdProblem))]
    public partial string Id { get; set; } = "";

    /// <summary>The field sprite the NPC is drawn from; the placeholder at first.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpriteProblem))]
    public partial string SpriteFile { get; set; }

    /// <summary>Why the id can't be used (<see cref="ObjectIds"/>), or null.</summary>
    public string? IdProblem => ObjectIds.ProblemOf(Id, _takenIds);

    public string? SpriteProblem => SpriteFile.Length == 0 ? "Put a field sprite PNG in content/art/field-sprites/ first." : null;

    /// <summary>One cell (decision 132), no tiles and no above-player zones, the default outline (decision 129).</summary>
    public ObjectDefinition ToDefinition() => new()
    {
        Id = Id,
        Width = 1,
        Height = 1,
        CollisionPolygon = _lowerHalfOutline,
        Category = NpcCategory,
        Tiles = [],
        FieldSpriteFile = SpriteFile,
        AbovePlayerZones = [],
    };
}
