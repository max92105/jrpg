using Jrpg.Core.Maps;

namespace Jrpg.Editor.Catalogs;

/// <summary>What a re-cut of a sheet touches (decision 116): its tiles, and the maps and objects drawing them.</summary>
/// <param name="Tiles">The tiles cut from the sheet, lowest id first.</param>
/// <param name="MapIds">The maps, saved or open, using any of them, sorted.</param>
/// <param name="ObjectIds">The objects made of any of them, sorted.</param>
internal sealed record SheetUsage(IReadOnlyList<TileDefinition> Tiles, IReadOnlyList<string> MapIds, IReadOnlyList<string> ObjectIds)
{
    /// <summary>A sheet nothing is cut from yet, as one being set up.</summary>
    public static SheetUsage None { get; } = new([], [], []);
}
