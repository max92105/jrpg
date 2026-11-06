using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Objects;

/// <summary>
/// The placed object panel's fields (decision 513): the one selected object's position and layer as typed, applied as
/// one undo step. They follow the selection and every change of the map, an undo included, so they never show a record
/// the map no longer holds.
/// </summary>
internal sealed partial class PlacedObjectEditing : ObservableObject
{
    private readonly CatalogDraft _catalog;
    private readonly ObjectPlacingViewModel _placing;
    private MapViewModel? _map;
    private PlacedObject? _shown;

    /// <param name="catalog">The objects' sizes, for the center the map must keep.</param>
    /// <param name="placing">The shown map and its selection.</param>
    public PlacedObjectEditing(CatalogDraft catalog, ObjectPlacingViewModel placing)
    {
        (_catalog, _placing) = (catalog, placing);
        placing.PropertyChanged += OnPlacingChanged;
        catalog.PropertyChanged += OnCatalogChanged;
    }

    /// <summary>Whether exactly one placed object is selected, so the panel shows it.</summary>
    public bool HasObject => _shown is not null;

    public string Uid => _shown?.Uid ?? "";

    /// <summary>The top-left's x in map cells, free (with a dot for decimals), as typed.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    public partial string XText { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    public partial string YText { get; set; } = "";

    /// <summary>The layer's z, any whole number (as the layer box takes), as typed.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    public partial string ZText { get; set; } = "";

    /// <summary>What stops Apply; null when it can apply, or with no object shown.</summary>
    public string? Problem => _shown is not { } shown ? null : ProblemOf(shown);

    private bool CanApply() => _shown is not null && Problem is null;

    /// <summary>
    /// Gives the object the typed position and layer as one undo step; nothing when they didn't change.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanApply))]
    private void Apply()
    {
        var shown = _shown!;
        var edited = shown with { Position = new Point2D(Coordinate(XText)!.Value, Coordinate(YText)!.Value), Z = Layer(ZText)!.Value };
        if (edited != shown)
        {
            _map!.Do(new ReplacePlacedObjectEdit(shown, edited));
        }
    }

    private string? ProblemOf(PlacedObject shown)
    {
        if (Coordinate(XText) is not { } x || Coordinate(YText) is not { } y)
        {
            return "X and Y must be numbers, with a dot for decimals.";
        }

        if (Layer(ZText) is null)
        {
            return "The layer must be a whole number.";
        }

        // Placing's own rule: an object whose center leaves the map is drawn clipped and couldn't be seen.
        var definition = _catalog.Current.Objects[shown.ObjectId];
        var center = new Point2D(x + (definition.Width / 2.0), y + (definition.Height / 2.0));
        return _map!.Map.Layout.Contains(GridCell.Containing(center)) ? null : "The object's center must stay on the map.";
    }

    private void OnPlacingChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ObjectPlacingViewModel.OpenMap))
        {
            _map?.PropertyChanged -= OnMapChanged;
            _map = _placing.OpenMap;
            _map?.PropertyChanged += OnMapChanged;
        }

        if (e.PropertyName is nameof(ObjectPlacingViewModel.OpenMap) or nameof(ObjectPlacingViewModel.SelectedObjectUids))
        {
            ShowSelected();
        }
    }

    // An object's size decides where its center falls, so an edit of it (or its undo) is checked again.
    private void OnCatalogChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CatalogDraft.Current))
        {
            OnPropertyChanged(nameof(Problem));
            ApplyCommand.NotifyCanExecuteChanged();
        }
    }

    private void OnMapChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MapViewModel.Map))
        {
            ShowSelected();
        }
    }

    // The record as it is now; typing not applied yet is dropped, so the fields never show another state of the map.
    private void ShowSelected()
    {
        _shown = _map is not null && _placing.SelectedObjectUids is [var uid] ? _map.Map.Layout.Objects.FirstOrDefault(placed => placed.Uid == uid) : null;
        XText = _shown?.Position.X.ToString(CultureInfo.InvariantCulture) ?? "";
        YText = _shown?.Position.Y.ToString(CultureInfo.InvariantCulture) ?? "";
        ZText = _shown?.Z.ToString(CultureInfo.InvariantCulture) ?? "";
        OnPropertyChanged(nameof(HasObject));
        OnPropertyChanged(nameof(Uid));
        OnPropertyChanged(nameof(Problem));
        ApplyCommand.NotifyCanExecuteChanged();
    }

    // A free position can be negative: an object near the left or top edge, centered on the map. TryParse also reads
    // "NaN" and "Infinity", which the content can't hold.
    private static double? Coordinate(string text) =>
        double.TryParse(text.Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value)
        && double.IsFinite(value)
            ? value
            : null;

    private static int? Layer(string text) =>
        int.TryParse(text.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var z) ? z : null;
}
