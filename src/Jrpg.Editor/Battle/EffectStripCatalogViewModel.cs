using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The Effect strips window's state (decision 659, ADR 0045): every image in the effects folder in name order with its
/// record or the unlisted pace, then each record whose image is missing (still editable and saved, decision 442).
/// Editing a strip's boxes lists it; Reset to unlisted puts the unlisted pace back, so OK drops its record. The problem
/// line is the chosen strip's own, then another strip's named by file, then Data's for the whole list; OK greys while
/// there is one. Nothing names a record by id, so no undo is refused. Reads no file itself: the folder's listing and an
/// image's size come in as lookups.
/// </summary>
internal sealed partial class EffectStripCatalogViewModel : ObservableObject
{
    /// <param name="current">The records as they are now.</param>
    /// <param name="images">The readable images in the effects folder.</param>
    /// <param name="sizeOf">An effects image's size by file name; null when the folder lacks it.</param>
    public EffectStripCatalogViewModel(
        ContentCatalog current, IReadOnlyList<string> images, Func<string, (int Width, int Height)?> sizeOf)
    {
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

    public IReadOnlyList<EffectStripViewModel> Strips { get; }

    /// <summary>What a strip's swing box offers: the effects images.</summary>
    public IReadOnlyList<string> SwingChoices { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(CanApply))]
    [NotifyCanExecuteChangedFor(nameof(ResetToUnlistedCommand))]
    public partial EffectStripViewModel? SelectedStrip { get; set; }

    public string? Problem =>
        SelectedStrip?.Problem
        ?? Strips.Where(strip => strip.Problem is not null).Select(strip => $"{strip.File}: {strip.Problem}").FirstOrDefault()
        ?? CatalogProblems.OfEffectStrips(ListedStrips());

    public bool CanApply => Problem is null;

    /// <summary>The records OK writes: every listed strip, sorted by file; an unlisted one has none.</summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>; OK is greyed then.</exception>
    public IReadOnlyList<EffectStripDefinition> ToStrips() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : ListedStrips();

    private List<EffectStripDefinition> ListedStrips() =>
        [.. Strips
            .Where(strip => strip.IsListed)
            .Select(strip => strip.ToStrip())
            .OrderBy(strip => strip.File, StringComparer.Ordinal)];

    /// <summary>Puts the chosen strip back at the unlisted pace, so OK writes no record for it.</summary>
    [RelayCommand(CanExecute = nameof(CanResetToUnlisted))]
    private void ResetToUnlisted()
    {
        SelectedStrip!.ResetToUnlisted();

        // A record already at the unlisted pace changes no box, so no box's notice says it was dropped.
        RefreshFromTheStrips();
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
        OnPropertyChanged(nameof(CanApply));
        ResetToUnlistedCommand.NotifyCanExecuteChanged();
    }
}
