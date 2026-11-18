using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.NewGame;

/// <summary>
/// The new game as the New game page types it (decision 968): its starting gold, its start's map, cell position and
/// facing, and the player's field sprite, written over the record it came from, so an untouched new game equals it.
/// <see cref="Problem"/> is the typed boxes first, then Data's message (decision 969). One record: its host has nothing
/// to choose (<see cref="IEditedCatalog"/>).
/// </summary>
internal sealed partial class NewGameViewModel : ObservableObject, IEditedCatalog
{
    private readonly NewGameDefinition _source;
    private readonly ContentCatalog _content;

    /// <param name="content">The draft as it is: the new game edited and the maps and field sprites it names.</param>
    public NewGameViewModel(ContentCatalog content)
    {
        _content = content;
        _source = content.NewGame;
        var start = _source.StartLocation;
        GoldText = TypedNumber.Text(_source.StartingGold);
        MapId = start.MapId;
        XText = TypedDecimal.Text(start.Position.X);
        YText = TypedDecimal.Text(start.Position.Y);
        Facing = start.Facing;
        FieldSpriteFile = _source.FieldSpriteFile;
        MapIds = [.. content.Maps.Keys.Append(start.MapId).Distinct().Order(StringComparer.Ordinal)];
        FieldSpriteFiles = [.. content.FieldSprites.Keys.Order(StringComparer.Ordinal)];
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string GoldText { get; set; }

    /// <summary>The start's map, the map box's chosen value.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string MapId { get; set; }

    /// <summary>
    /// The map box's choices: the draft's maps by id, and the start's map when the draft lacks it, so the box still
    /// shows it and Data's message names it.
    /// </summary>
    public IReadOnlyList<string> MapIds { get; }

    /// <summary>The start's x in cells as typed: 2.5 is the middle of the third column.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string XText { get; set; }

    /// <summary>The start's y in cells as typed: 2.5 is the middle of the third row.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string YText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial Facing Facing { get; set; }

    public IReadOnlyList<Facing> Facings { get; } = Enum.GetValues<Facing>();

    /// <summary>
    /// The player's field sprite as typed or picked; the editable box keeps a file the draft has no record of.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string FieldSpriteFile { get; set; }

    /// <summary>The sprite box's choices: the files the draft's field sprites set up (ADR 0043).</summary>
    public IReadOnlyList<string> FieldSpriteFiles { get; }

    /// <summary>
    /// Why the game would refuse the new game as typed: a box that isn't a number first, in the page's order, then
    /// Data's message (<see cref="CatalogProblems.OfNewGame"/>); null when the game would take it.
    /// </summary>
    public string? Problem => BoxProblem ?? CatalogProblems.OfNewGame(AsTyped(), _content);

    /// <summary>Always null: the new game is one record, so there is nothing to choose.</summary>
    public string? ChosenKey => null;

    // In the page's order: the gold above the start.
    private string? BoxProblem =>
        TypedNumber.ProblemOf("Starting gold", GoldText)
        ?? TypedDecimal.ProblemOf("Start x", XText)
        ?? TypedDecimal.ProblemOf("Start y", YText);

    /// <summary>Keeps everything as it is: the new game is the page's one record.</summary>
    public void Choose(string key)
    {
    }

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the page's host applies only without one.
    /// </exception>
    public NewGameDefinition ToNewGame() => Problem is { } problem ? throw new InvalidOperationException(problem) : AsTyped();

    // Read only once the boxes are numbers; the sprite's name trimmed, as the members' art names are.
    private NewGameDefinition AsTyped() =>
        _source with
        {
            StartingGold = TypedNumber.Parse(GoldText)!.Value,
            StartLocation = new MapLocation(
                MapId,
                new Point2D(TypedDecimal.Parse(XText)!.Value, TypedDecimal.Parse(YText)!.Value),
                Facing),
            FieldSpriteFile = FieldSpriteFile.Trim(),
        };
}
