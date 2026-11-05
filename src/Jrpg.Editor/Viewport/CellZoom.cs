using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Jrpg.Editor.Viewport;

/// <summary>
/// How big a cell is shown on the Tiles panel's sheet or the composer's canvas: a size in fixed steps, stepped between
/// bounds, and the scale and scroll offset it gives (decisions 481, 483). Pure, as <see cref="ViewportTransform"/> is
/// for the map, so the views only apply it.
/// </summary>
internal sealed partial class CellZoom : ObservableObject
{
    private readonly int _startCellPixels;

    /// <param name="startCellPixels">
    /// The size it opens at and <see cref="ResetCommand"/> goes back to; one of <see cref="Steps"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">A start that isn't one of <see cref="Steps"/>.</exception>
    public CellZoom(int startCellPixels)
    {
        if (!Steps.Contains(startCellPixels))
        {
            throw new ArgumentOutOfRangeException(nameof(startCellPixels), startCellPixels, $"A cell zoom starts on one of its steps: {string.Join(", ", Steps)}.");
        }

        _startCellPixels = startCellPixels;
        CellPixels = startCellPixels;
    }

    /// <summary>Screen pixels per cell: whole factors, 1x to 12x, for the 16 px sheets (decision 444).</summary>
    public static IReadOnlyList<int> Steps { get; } = [16, 32, 48, 64, 96, 128, 192];

    /// <summary>The cell's shown size in screen pixels, one of <see cref="Steps"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CellPixelsText))]
    [NotifyCanExecuteChangedFor(nameof(ZoomInCommand), nameof(ZoomOutCommand))]
    public partial int CellPixels { get; private set; }

    public string CellPixelsText => string.Create(CultureInfo.InvariantCulture, $"{CellPixels} px");

    private bool CanZoomIn => CellPixels < Steps[^1];

    private bool CanZoomOut => CellPixels > Steps[0];

    /// <summary>
    /// The scroll offset on one axis that keeps the content point under <paramref name="anchor"/> (a position in the
    /// viewer) where it was after the scale changes; never below 0, where the viewer stops.
    /// </summary>
    public static double OffsetKeeping(double offset, double anchor, double oldScale, double newScale) =>
        Math.Max(0, ((offset + anchor) / oldScale * newScale) - anchor);

    /// <summary>
    /// The scale that shows a source cell of <paramref name="sourceCellPixels"/> at <see cref="CellPixels"/>.
    /// </summary>
    public double ScaleFor(double sourceCellPixels) => CellPixels / sourceCellPixels;

    [RelayCommand(CanExecute = nameof(CanZoomIn))]
    private void ZoomIn() => CellPixels = Steps.First(step => step > CellPixels);

    [RelayCommand(CanExecute = nameof(CanZoomOut))]
    private void ZoomOut() => CellPixels = Steps.Last(step => step < CellPixels);

    [RelayCommand]
    private void Reset() => CellPixels = _startCellPixels;
}
