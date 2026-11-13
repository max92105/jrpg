using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;

namespace Jrpg.Editor.Objects;

/// <summary>
/// The New NPC dialog (ADR 0015): the id typed and the sprite chosen among the set-up sheets (ADR 0043), the
/// placeholder first; and the same dialog as Change NPC sprite on an NPC that exists, its id fixed.
/// </summary>
internal sealed partial class NewNpcViewModel : ObservableObject
{
    /// <summary>The sprite every NPC shares until real art exists (decision 69).</summary>
    public const string PlaceholderSprite = "npc_placeholder.png";

    private const string NpcCategory = "NPC";

    // Decision 129: the lower half of the cell blocks, so the player walks behind an NPC's head.
    private static readonly IReadOnlyList<Point2D> _lowerHalfOutline =
        [new Point2D(0.2, 0.5), new Point2D(0.8, 0.5), new Point2D(0.8, 1), new Point2D(0.2, 1)];

    private readonly IReadOnlyCollection<string> _takenIds;
    private readonly IReadOnlyDictionary<string, FieldSpriteSheet> _sheets;
    private readonly ObjectDefinition? _changed;

    /// <param name="takenIds">The object ids already in the catalog.</param>
    /// <param name="sheets">The set-up sheets an NPC may use.</param>
    public NewNpcViewModel(IReadOnlyCollection<string> takenIds, IReadOnlyList<FieldSpriteSheet> sheets)
        : this(takenIds, sheets, changed: null)
    {
    }

    private NewNpcViewModel(
        IReadOnlyCollection<string> takenIds, IReadOnlyList<FieldSpriteSheet> sheets, ObjectDefinition? changed)
    {
        _takenIds = takenIds;
        _sheets = sheets.ToDictionary(sheet => sheet.File, StringComparer.Ordinal);
        _changed = changed;
        SpriteFiles = [.. sheets
            .Select(sheet => sheet.File)
            .OrderBy(file => file == PlaceholderSprite ? 0 : 1)
            .ThenBy(file => file, StringComparer.Ordinal)];
        Id = changed?.Id ?? "";
        SpriteFile = changed?.FieldSpriteFile ?? (SpriteFiles.Count > 0 ? SpriteFiles[0] : "");
    }

    /// <summary>The set-up sheets' files, the placeholder first, then by name.</summary>
    public IReadOnlyList<string> SpriteFiles { get; }

    /// <summary>Whether the dialog changes an NPC's sprite, its id fixed (maps and saves name it).</summary>
    public bool IsIdFixed => _changed is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IdProblem))]
    public partial string Id { get; set; }

    /// <summary>The field sprite the NPC is drawn from; the placeholder at first.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpriteProblem), nameof(ChosenSheet))]
    public partial string SpriteFile { get; set; }

    /// <summary>The chosen sprite's record, for the preview; null while none is chosen.</summary>
    public FieldSpriteSheet? ChosenSheet => _sheets.GetValueOrDefault(SpriteFile);

    /// <summary>Why the id can't be used (<see cref="ObjectIds"/>), or null; always null with the id fixed.</summary>
    public string? IdProblem => IsIdFixed ? null : ObjectIds.ProblemOf(Id, _takenIds);

    public string? SpriteProblem =>
        SpriteFiles.Count == 0 ? "Set up a character sprite first: Objects > Character sprites..."
        : ChosenSheet is null ? $"{SpriteFile} has no record; set it up in Objects > Character sprites..."
        : null;

    /// <summary>Change NPC sprite: the dialog on an NPC that exists, keeping everything but the sprite.</summary>
    /// <param name="npc">The object changed, one drawn from a field sprite.</param>
    /// <param name="sheets">The set-up sheets it may use.</param>
    public static NewNpcViewModel ForChange(ObjectDefinition npc, IReadOnlyList<FieldSpriteSheet> sheets) => new([], sheets, npc);

    /// <summary>
    /// A new NPC: one cell (decision 132), no tiles and no above-player zones, the default outline (decision 129); or
    /// the changed NPC with its new sprite, outline, category, size and zones kept.
    /// </summary>
    public ObjectDefinition ToDefinition() => _changed is { } changed
        ? changed with { FieldSpriteFile = SpriteFile }
        : new ObjectDefinition
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
