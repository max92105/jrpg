using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps;

/// <summary>
/// The folder dialog's choice (decisions 532-534): a path picked among the folders the list has, or typed, for New
/// folder, Move to folder and Rename folder; <see cref="Top"/>, or nothing, is the top of the list.
/// </summary>
internal sealed partial class MapFolderChoiceViewModel : ObservableObject
{
    public const string Top = "(top of the list)";

    /// <param name="knownFolders">
    /// The folders of the saved and open maps and the session's, any order, repeats allowed.
    /// </param>
    /// <param name="startPath">What the box holds at first; null for the top.</param>
    public MapFolderChoiceViewModel(IEnumerable<string> knownFolders, string? startPath)
    {
        Folders = [Top, .. knownFolders.SelectMany(FolderAndItsParents).Distinct(StringComparer.Ordinal).OrderBy(folder => folder, StringComparer.OrdinalIgnoreCase)];
        PathText = startPath ?? Top;
    }

    /// <summary>
    /// The paths to pick from: the top first, then every folder shown, its parents included, by name.
    /// </summary>
    public IReadOnlyList<string> Folders { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(CanApply))]
    public partial string PathText { get; set; }

    /// <summary>
    /// Why the text can't be a folder path (<see cref="MapFolderPath"/>); null when it can, or means the top.
    /// </summary>
    public string? Problem => IsTop ? null : MapFolderPath.ProblemOf(PathText.Trim());

    public bool CanApply => Problem is null;

    private bool IsTop => PathText.Trim() is "" or Top;

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
