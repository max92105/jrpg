using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Objects.Composing;

/// <summary>
/// One entry of the composer's Level box: a level and how the box names it, marked while a cell is painted on it
/// (decisions 490, 491).
/// </summary>
internal sealed partial class LevelChoice(int level) : ObservableObject
{
    public int Level { get; } = level;

    public string Name => Level == 0 ? "0 (base)" : Level.ToString(CultureInfo.InvariantCulture);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Text))]
    public partial bool IsPainted { get; set; }

    /// <summary>
    /// What the box shows: the name, then a dot while the level holds a painted cell (placeholder look).
    /// </summary>
    public string Text => IsPainted ? $"{Name} •" : Name;

    /// <summary>
    /// One entry per level, from <see cref="ObjectTile.MaxLevel"/> down to <see cref="ObjectTile.MinLevel"/>: the box
    /// reads like the stack, its top level first.
    /// </summary>
    public static IReadOnlyList<LevelChoice> TopDown() =>
        [.. Enumerable.Range(ObjectTile.MinLevel, ObjectTile.MaxLevel - ObjectTile.MinLevel + 1).Reverse().Select(level => new LevelChoice(level))];
}
