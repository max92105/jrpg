using System.Globalization;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Objects.Composing;

/// <summary>One entry of the composer's Level box: a level and how the box names it (decision 490).</summary>
internal sealed class LevelChoice(int level)
{
    public int Level { get; } = level;

    public string Name => Level == 0 ? "0 (base)" : Level.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// One entry per level, from <see cref="ObjectTile.MaxLevel"/> down to <see cref="ObjectTile.MinLevel"/>: the box
    /// reads like the stack, its top level first.
    /// </summary>
    public static IReadOnlyList<LevelChoice> TopDown() =>
        [.. Enumerable.Range(ObjectTile.MinLevel, ObjectTile.MaxLevel - ObjectTile.MinLevel + 1).Reverse().Select(level => new LevelChoice(level))];
}
