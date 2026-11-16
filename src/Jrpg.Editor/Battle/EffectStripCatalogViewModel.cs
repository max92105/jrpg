using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The Effect strips page's state (decision 659, ADR 0045): every image in the effects folder in name order with its
/// record or the unlisted pace, then each record whose image is missing (still editable and saved, decision 442).
/// Editing a strip's boxes lists it; Reset to unlisted puts the unlisted pace back, so OK drops its record. The problem
/// line is the chosen strip's own, then another strip's named by file, then Data's for the whole list; OK greys while
/// there is one. Nothing names a record by id, so no undo is refused, and a strip is chosen by its file. Reads no file
/// itself: the folder's listing and an image's size come in as lookups. An import into the folder enters the session
/// (decision 988): each imported image's row is built again from its record at its new size, its typed pace taken.
/// </summary>
internal sealed partial class EffectStripCatalogViewModel : ObservableObject, IEditedCatalog
{
    private readonly ContentCatalog _current;
    private readonly Func<string, (int Width, int Height)?> _sizeOf;

    /// <param name="current">The records as they are now.</param>
    /// <param name="images">The readable images in the effects folder.</param>
    /// <param name="sizeOf">An effects image's size by file name; null when the folder lacks it.</param>
    public EffectStripCatalogViewModel(
        ContentCatalog current, IReadOnlyList<string> images, Func<string, (int Width, int Height)?> sizeOf)
    {
        (_current, _sizeOf) = (current, sizeOf);
        var inFolder = images.Order(StringComparer.Ordinal).Select(file => current.EffectStrips.TryGetValue(file, out var strip)
            ? EffectStripViewModel.From(strip, sizeOf)
            : EffectStripViewModel.Unlisted(file, sizeOf));
        var missing = current.EffectStrips.Values
            .Where(strip => !images.Contains(strip.File, StringComparer.Ordinal))
            .OrderBy(strip => strip.File, StringComparer.Ordinal)
            .Select(strip => EffectStripViewModel.From(strip, sizeOf));
        Strips = [.. inFolder.Concat(missing)];
        SwingChoices = [.. images.Order(StringComparer.Ordinal)];
        foreach (var strip in Strips)
        {
            strip.PropertyChanged += OnStripChanged;
        }

        SelectedStrip = Strips.Count > 0 ? Strips[0] : null;
    }

    public ObservableCollection<EffectStripViewModel> Strips { get; }

    /// <summary>What a strip's swing box offers: the effects images, in name order.</summary>
    public ObservableCollection<string> SwingChoices { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    [NotifyCanExecuteChangedFor(nameof(ResetToUnlistedCommand))]
    public partial EffectStripViewModel? SelectedStrip { get; set; }

    public string? Problem =>
        SelectedStrip?.Problem
        ?? Strips.Where(strip => strip.Problem is not null).Select(strip => $"{strip.File}: {strip.Problem}").FirstOrDefault()
        ?? CatalogProblems.OfEffectStrips(ListedStrips());

    public string? ChosenKey => SelectedStrip?.File;

    public void Choose(string key) => SelectedStrip = Strips.FirstOrDefault(strip => strip.File == key) ?? SelectedStrip;

    /// <summary>The records OK writes: every listed strip, sorted by file; an unlisted one has none.</summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>; OK is greyed then.</exception>
    public IReadOnlyList<EffectStripDefinition> ToStrips() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : ListedStrips();

    private List<EffectStripDefinition> ListedStrips() =>
        [.. Strips
            .Where(strip => strip.IsListed)
            .Select(strip => strip.ToStrip())
            .OrderBy(strip => strip.File, StringComparer.Ordinal)];

    /// <summary>
    /// After an import into the effects folder (decision 988): each imported image's row is built again from the
    /// draft's record, or at the unlisted pace, at its new size, its earlier typed boxes dropped, and put back in name
    /// order among the folder's images; a GIF's typed pace is its frame time, and a hit frame past the new frames goes
    /// back to 0. A new image is offered as a swing; the last one imported is chosen.
    /// </summary>
    /// <param name="result">What the import copied, the typed paces among its loops.</param>
    public void TakeImport(SheetImportResult result)
    {
        if (result.LastImported is not { } last)
        {
            return;
        }

        foreach (var file in result.Imported)
        {
            TakeImportedImage(file, result.Animations.GetValueOrDefault(file));
        }

        SelectedStrip = Strips.Single(strip => strip.File == last);
    }

    /// <summary>
    /// The page's line after an import: the import's own, then, once an image was copied, that it is in the folder
    /// already while the records wait for OK (decision 988).
    /// </summary>
    public static string ImportLineOf(SheetImportResult result) =>
        result.Imported.Count == 0
            ? result.Summary
            : $"{result.Summary} The images are in the effects folder now, never undone; a replaced strip starts again "
                + "from its record, and the records wait for OK.";

    /// <summary>Puts the chosen strip back at the unlisted pace, so OK writes no record for it.</summary>
    [RelayCommand(CanExecute = nameof(CanResetToUnlisted))]
    private void ResetToUnlisted()
    {
        SelectedStrip!.ResetToUnlisted();

        // A record already at the unlisted pace changes no box, so no box's notice says it was dropped.
        RefreshFromTheStrips();
    }

    private void TakeImportedImage(string file, TileAnimation? loop)
    {
        var strip = _current.EffectStrips.TryGetValue(file, out var record)
            ? EffectStripViewModel.From(record, _sizeOf)
            : EffectStripViewModel.Unlisted(file, _sizeOf);
        if (loop is not null)
        {
            strip.FrameMillisecondsText = TypedNumber.Text(loop.FrameMilliseconds);
        }

        if (strip.HitFrame >= strip.FrameCount)
        {
            strip.HitFrameText = TypedNumber.Text(0);
        }

        if (Strips.SingleOrDefault(row => row.File == file) is { } replaced)
        {
            replaced.PropertyChanged -= OnStripChanged;
            Strips.Remove(replaced);
        }

        strip.PropertyChanged += OnStripChanged;
        // Counting the folder's rows before it works since the rows whose image is missing always come after them.
        Strips.Insert(Strips.Count(row => row.FrameCount is not null && string.CompareOrdinal(row.File, file) < 0), strip);
        if (!SwingChoices.Contains(file))
        {
            SwingChoices.Insert(SwingChoices.Count(choice => string.CompareOrdinal(choice, file) < 0), file);
        }
    }

    private bool CanResetToUnlisted() => SelectedStrip is { IsListed: true };

    // A strip's typed boxes move the problem line and whether it can be reset; its own computed notices are skipped.
    private void OnStripChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(EffectStripViewModel.FrameMillisecondsText) or nameof(EffectStripViewModel.HitFrameText)
            or nameof(EffectStripViewModel.SwingFile))
        {
            RefreshFromTheStrips();
        }
    }

    private void RefreshFromTheStrips()
    {
        OnPropertyChanged(nameof(Problem));
        ResetToUnlistedCommand.NotifyCanExecuteChanged();
    }
}
