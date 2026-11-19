using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Settings;

namespace Jrpg.Editor.Encounters;

/// <summary>
/// The Encounters page's Link dialog: the map to link the encounter to, picked among those not rolling it yet. A map
/// without random battles gets them here, so its odds are typed, and its battle background picked when it names none
/// (the game refuses random battles without one, decision 892).
/// </summary>
internal sealed partial class MapLinkViewModel : ObservableObject
{
    // What a map new to random battles starts from (placeholder odds, the developer's to retype): a battle about every
    // 20 steps, none in the 5 after one.
    private const int StartingOneInSteps = 20;
    private const int StartingGraceSteps = 5;

    private readonly string _encounterId;
    private readonly Dictionary<string, MapDefinition> _maps;

    /// <param name="encounterId">The encounter to link.</param>
    /// <param name="unlinkedMaps">The maps whose random battles don't pick it, as they are now.</param>
    /// <param name="backgrounds">The images in the backgrounds folder, for a map naming none.</param>
    public MapLinkViewModel(string encounterId, IReadOnlyList<MapDefinition> unlinkedMaps, IReadOnlyList<string> backgrounds)
    {
        _encounterId = encounterId;
        _maps = unlinkedMaps.ToDictionary(map => map.Id, StringComparer.Ordinal);
        Maps = MapLinkChoice.AllOf(unlinkedMaps);
        Backgrounds = backgrounds;
    }

    public string Title => $"Link {_encounterId} to a map";

    public IReadOnlyList<MapLinkChoice> Maps { get; }

    public IReadOnlyList<string> Backgrounds { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AsksOdds), nameof(AsksBackground), nameof(Problem), nameof(CanApply))]
    public partial MapLinkChoice? SelectedMap { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(CanApply))]
    public partial string OneInStepsText { get; set; } = StartingOneInSteps.ToString(CultureInfo.InvariantCulture);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(CanApply))]
    public partial string GraceStepsText { get; set; } = StartingGraceSteps.ToString(CultureInfo.InvariantCulture);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(CanApply))]
    public partial string? SelectedBackground { get; set; }

    /// <summary>True while the picked map has no random battles yet, so the odds boxes show.</summary>
    public bool AsksOdds => Picked is { Settings.RandomEncounters: null };

    /// <summary>True while the picked map gets random battles here and names no battle background.</summary>
    public bool AsksBackground => Picked is { Settings: { RandomEncounters: null, BattleBackgroundFile: null } };

    /// <summary>
    /// Why OK is greyed: no map picked, or an odds box or the background missing; null when it can link.
    /// </summary>
    public string? Problem
    {
        get
        {
            if (Picked is null)
            {
                return "Pick a map.";
            }

            if (AsksOdds && (TypedOneInSteps is null || TypedGraceSteps is null))
            {
                return "The odds and the grace steps must be whole numbers, 1 or more.";
            }

            return AsksBackground && SelectedBackground is null ? "Random battles need a battle background." : null;
        }
    }

    public bool CanApply => Problem is null;

    /// <summary>The picked map's id and its settings with the encounter linked.</summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public MapSettings ToSettings()
    {
        if (Problem is { } problem)
        {
            throw new InvalidOperationException(problem);
        }

        var settings = Picked!.Settings;
        if (!AsksOdds)
        {
            return MapEncounterLink.Linked(settings, _encounterId);
        }

        return settings with
        {
            BattleBackgroundFile = settings.BattleBackgroundFile ?? SelectedBackground,
            RandomEncounters = new RandomEncounterTable
            {
                OneInSteps = TypedOneInSteps!.Value,
                GraceSteps = TypedGraceSteps!.Value,
                EncounterIds = [_encounterId],
            },
        };
    }

    private MapDefinition? Picked => SelectedMap is { } choice ? _maps[choice.MapId] : null;

    private int? TypedOneInSteps => TypedCounts.Parse(OneInStepsText, int.MaxValue);

    private int? TypedGraceSteps => TypedCounts.Parse(GraceStepsText, int.MaxValue);
}
