using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Backgrounds;

/// <summary>
/// The Battle backgrounds page's state (decisions 901, 902): every image in the backgrounds folder in name order, still
/// or animated by its record, then each record whose image is missing (still listed and saved, decision 442). The
/// problem line is the chosen background's own, then another's named by file, then Data's for the whole list; Apply
/// greys while there is one. Nothing names a record by id, so no undo is refused, and a background is chosen by its
/// file. Reads no file itself: the folder's listing and an image's size come in as lookups.
/// </summary>
internal sealed partial class BattleBackgroundCatalogViewModel : ObservableObject, IEditedCatalog
{
    /// <param name="current">The records as they are now.</param>
    /// <param name="images">The readable images in the backgrounds folder, in any order.</param>
    /// <param name="sizeOf">A backgrounds image's size by file name; null when the folder lacks it.</param>
    public BattleBackgroundCatalogViewModel(
        ContentCatalog current, IReadOnlyList<string> images, Func<string, (int Width, int Height)?> sizeOf)
    {
        var rows = ArtImageRows.Of([.. images.Order(StringComparer.Ordinal)], current.AnimatedBackgrounds.Keys);
        Backgrounds = [.. rows.Select(row => BattleBackgroundViewModel.From(
            row.File, current.AnimatedBackgrounds.GetValueOrDefault(row.File), SizeOf(row, sizeOf)))];
        foreach (var background in Backgrounds)
        {
            background.PropertyChanged += OnBackgroundChanged;
        }

        SelectedBackground = Backgrounds.Count > 0 ? Backgrounds[0] : null;
    }

    public ObservableCollection<BattleBackgroundViewModel> Backgrounds { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial BattleBackgroundViewModel? SelectedBackground { get; set; }

    public string? Problem =>
        ArtImageRowProblem.FirstOf(SelectedBackground, Backgrounds, row => row.File, row => row.Problem)
        ?? CatalogProblems.OfAnimatedBackgrounds(AnimatedRecords());

    public string? ChosenKey => SelectedBackground?.File;

    public void Choose(string key) =>
        SelectedBackground = Backgrounds.FirstOrDefault(background => background.File == key) ?? SelectedBackground;

    /// <summary>The records Apply writes: every animated background's, sorted by file; a still one has none.</summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>; Apply is greyed then.</exception>
    public IReadOnlyList<AnimatedBackgroundDefinition> ToRecords() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : AnimatedRecords();

    // Read only once no row has a problem, as a row's record throws on one.
    private List<AnimatedBackgroundDefinition> AnimatedRecords() =>
        [.. Backgrounds
            .Select(background => background.ToRecord())
            .OfType<AnimatedBackgroundDefinition>()
            .OrderBy(record => record.File, StringComparer.Ordinal)];

    // A record's file the listing lacks is never looked up: Windows would read an image named in another case, which
    // the game's exact-case check refuses (decision 895).
    private static (int Width, int Height)? SizeOf(ArtImageRow row, Func<string, (int Width, int Height)?> sizeOf) =>
        row.IsImageMissing ? null : sizeOf(row.File);

    // A row's typed frame time moves the problem line; its own computed notices are skipped.
    private void OnBackgroundChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BattleBackgroundViewModel.FrameMillisecondsText))
        {
            OnPropertyChanged(nameof(Problem));
        }
    }
}
