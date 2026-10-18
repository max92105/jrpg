using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps.Settings;

/// <summary>
/// The map settings dialog: display name, size and random battles, edited apart from the map until applied. The id
/// can't change (question 70), since saves and warps refer to it. Numbers are kept as typed and parsed here, so a
/// box left empty or holding a typo is refused instead of keeping the last number that happened to parse.
/// </summary>
internal sealed partial class MapSettingsViewModel : ObservableObject
{
    // Legacy MapPropertiesWindow limits.
    public const int MaxSize = 1000;

    private readonly (int Width, int Height) _currentSize;
    private readonly MapSettings _currentSettings;

    /// <param name="map">The map as it is.</param>
    /// <param name="encounterIds">Every encounter random battles can pick from.</param>
    public MapSettingsViewModel(MapDefinition map, IEnumerable<string> encounterIds)
    {
        MapId = map.Id;
        _currentSettings = map.Settings;
        _currentSize = (map.Layout.Width, map.Layout.Height);
        DisplayName = map.Settings.DisplayName;
        WidthText = Text(map.Layout.Width);
        HeightText = Text(map.Layout.Height);
        var table = map.Settings.RandomEncounters;
        HasRandomEncounters = table is not null;

        // A map without battles starts from the lowest values the game accepts; the odds are the developer's to set.
        OneInStepsText = Text(table?.OneInSteps ?? 1);
        GraceStepsText = Text(table?.GraceSteps ?? 1);
        Encounters = [.. encounterIds.Order(StringComparer.Ordinal).Select(id => new EncounterChoice(id) { IsChosen = table?.EncounterIds.Contains(id) == true })];
    }

    public string MapId { get; }

    public IReadOnlyList<EncounterChoice> Encounters { get; }

    [ObservableProperty]
    public partial string DisplayName { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewSize), nameof(IsShrinking), nameof(IsResizing), nameof(SizeProblem))]
    public partial string WidthText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewSize), nameof(IsShrinking), nameof(IsResizing), nameof(SizeProblem))]
    public partial string HeightText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OddsProblem))]
    public partial bool HasRandomEncounters { get; set; }

    /// <summary>Each step past the grace steps starts a battle with a chance of 1 in this many.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OddsProblem))]
    public partial string OneInStepsText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OddsProblem))]
    public partial string GraceStepsText { get; set; }

    /// <summary>The size typed in; null when it isn't two whole numbers from 1 to <see cref="MaxSize"/>.</summary>
    public (int Width, int Height)? NewSize =>
        ParseCount(WidthText, MaxSize) is { } width && ParseCount(HeightText, MaxSize) is { } height ? (width, height) : null;

    /// <summary>Whether applying drops cells along the right or bottom edge, and with them what's on them.</summary>
    public bool IsShrinking => NewSize is { } size && (size.Width < _currentSize.Width || size.Height < _currentSize.Height);

    public bool IsResizing => NewSize is { } size && size != _currentSize;

    /// <summary>Why the size can't be applied; null when it can.</summary>
    public string? SizeProblem => NewSize is null ? $"Width and height must be whole numbers from 1 to {MaxSize}." : null;

    /// <summary>Why the battle odds can't be applied; null when they can or the map has no random battles.</summary>
    public string? OddsProblem =>
        HasRandomEncounters && (ParseCount(OneInStepsText, int.MaxValue) is null || ParseCount(GraceStepsText, int.MaxValue) is null)
            ? "The battle odds and the steps without battles must be whole numbers of at least 1."
            : null;

    /// <summary>Whether the name or the random battles differ from the map's.</summary>
    public bool AreSettingsChanged => !AreSame(ToSettings(), _currentSettings);

    /// <summary>The settings as edited, once <see cref="OddsProblem"/> is null; the encounter list is checked when the map is saved.</summary>
    public MapSettings ToSettings() => new()
    {
        Id = MapId,
        DisplayName = DisplayName.Trim(),
        RandomEncounters = HasRandomEncounters
            ? new RandomEncounterTable
            {
                OneInSteps = ParseCount(OneInStepsText, int.MaxValue) ?? 0,
                GraceSteps = ParseCount(GraceStepsText, int.MaxValue) ?? 0,
                EncounterIds = [.. Encounters.Where(choice => choice.IsChosen).Select(choice => choice.EncounterId)],
            }
            : null,
    };

    private static string Text(int number) => number.ToString(CultureInfo.InvariantCulture);

    // Digits only, from 1 to the maximum.
    private static int? ParseCount(string text, int maximum) =>
        int.TryParse(text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number >= 1 && number <= maximum ? number : null;

    // Records compare lists by reference, so the encounters are compared as sets: their order means nothing to the game.
    private static bool AreSame(MapSettings edited, MapSettings current) =>
        edited.DisplayName == current.DisplayName
        && (edited.RandomEncounters, current.RandomEncounters) switch
        {
            (null, null) => true,
            ({ } left, { } right) => left.OneInSteps == right.OneInSteps && left.GraceSteps == right.GraceSteps
                && left.EncounterIds.ToHashSet().SetEquals(right.EncounterIds),
            _ => false,
        };
}
