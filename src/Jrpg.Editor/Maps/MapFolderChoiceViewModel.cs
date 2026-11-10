using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps;

/// <summary>
/// The folder dialog's choice (decisions 532-534): a path picked among the folders the list has, or typed, for New
/// folder, Move to folder and Rename folder; <see cref="Top"/>, or nothing, is the top of the list, where no new
/// folder can go.
/// </summary>
internal sealed partial class MapFolderChoiceViewModel : ObservableObject
{
    public const string Top = "(top of the list)";

    private const string NeedsAName = "Type the new folder's name.";

    private readonly string? _topProblem;

    /// <param name="knownFolders">
    /// The folders of the saved and open maps and the session's, any order, repeats allowed.
    /// </param>
    /// <param name="startPath">What the box holds at first; null for the top.</param>
    public MapFolderChoiceViewModel(IEnumerable<string> knownFolders, string? startPath)
        : this(knownFolders, startPath ?? Top, topProblem: null)
    {
    }

    private MapFolderChoiceViewModel(IEnumerable<string> knownFolders, string startText, string? topProblem)
    {
        var known = knownFolders.SelectMany(FolderAndItsParents).Distinct(StringComparer.Ordinal).OrderBy(folder => folder, StringComparer.OrdinalIgnoreCase);
        Folders = topProblem is null ? [Top, .. known] : [.. known];
        PathText = startText;
        _topProblem = topProblem;
    }

    /// <summary>
    /// The paths to pick from: the top first (unless a new folder is named), then every folder shown, its parents
    /// included, by name.
    /// </summary>
    public IReadOnlyList<string> Folders { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(CanApply))]
    public partial string PathText { get; set; }

    /// <summary>
    /// Why the text can't be a folder path (<see cref="MapFolderPath"/>), or, for a new folder, that none is typed yet;
    /// null when it can, or means the top.
    /// </summary>
    public string? Problem => IsTop ? _topProblem : MapFolderPath.ProblemOf(PathText.Trim());

    public bool CanApply => Problem is null;

    private bool IsTop => PathText.Trim() is "" or Top;

    /// <summary>
    /// New folder: the box offers the parent's path with the separator after it, or nothing at the top.
    /// </summary>
    /// <param name="knownFolders">As the constructor's.</param>
    /// <param name="parentPath">The folder the new one goes under; null for one at the top.</param>
    public static MapFolderChoiceViewModel ForNewFolder(IEnumerable<string> knownFolders, string? parentPath) =>
        new(knownFolders, parentPath is null ? "" : parentPath + MapFolderPath.Separator, NeedsAName);

    /// <summary>The path chosen; null for the top of the list.</summary>
    /// <exception cref="InvalidOperationException">The text has a problem (OK is greyed then).</exception>
    public string? ToPath()
    {
        if (Problem is { } problem)
        {
            throw new InvalidOperationException(problem);
        }

        return IsTop ? null : PathText.Trim();
    }

    // "Act 1/Village" is picked from under "Act 1", so the list offers both.
    private static IEnumerable<string> FolderAndItsParents(string path)
    {
        var names = MapFolderPath.NamesOf(path);
        return Enumerable.Range(1, names.Count).Select(depth => string.Join(MapFolderPath.Separator, names.Take(depth)));
    }
}
