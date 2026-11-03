using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Art;

/// <summary>
/// Reads a Tiled tileset file (<c>.tsx</c>) from Tiled's documented format: <c>&lt;tileset tilewidth tileheight columns
/// margin spacing&gt;</c>, its <c>&lt;image source&gt;</c>, and per animated tile <c>&lt;tile
/// id&gt;&lt;animation&gt;</c> of <c>&lt;frame tileid duration&gt;</c>, a tile id being <c>row * columns + column</c>.
/// </summary>
public static class TiledTilesetReader
{
    /// <exception cref="ContentValidationException">
    /// The file isn't a Tiled tileset, or lacks its tile size, columns or image, naming the file.
    /// </exception>
    public static TiledTileset Read(string tsxPath)
    {
        var fileName = Path.GetFileName(tsxPath);
        var tileset = LoadTileset(tsxPath, fileName);
        var columns = PositiveNumberOf(tileset, "columns", fileName);
        var imageSource = tileset.Element("image")?.Attribute("source")?.Value
            ?? throw new ContentValidationException($"{fileName}: the tileset has no <image source>.");
        var (animations, refusal) = ReadAnimations(tileset, columns, fileName);
        return new TiledTileset
        {
            ImageFile = Path.GetFileName(imageSource),
            TileWidth = PositiveNumberOf(tileset, "tilewidth", fileName),
            TileHeight = PositiveNumberOf(tileset, "tileheight", fileName),
            Margin = OptionalNumberOf(tileset, "margin", fileName),
            Spacing = OptionalNumberOf(tileset, "spacing", fileName),
            Animations = animations,
            Refusal = refusal,
        };
    }

    private static XElement LoadTileset(string tsxPath, string fileName)
    {
        XElement? root;
        try
        {
            root = XDocument.Load(tsxPath).Root;
        }
        catch (XmlException exception)
        {
            throw new ContentValidationException($"{fileName}: not a Tiled tileset ({exception.Message})", exception);
        }

        return root is { Name.LocalName: "tileset" } ? root : throw new ContentValidationException($"{fileName}: not a Tiled tileset (no <tileset> root).");
    }

    // Tile ids order the animations, so the refusal names the same tile on every read.
    private static (Dictionary<(int Column, int Row), TileAnimation> Animations, string? Refusal) ReadAnimations(XElement tileset, int columns, string fileName)
    {
        var animations = new Dictionary<(int Column, int Row), TileAnimation>();
        string? refusal = null;
        var animated = tileset.Elements("tile").Where(tile => tile.Element("animation") is not null)
            .Select(tile => (Id: NumberOf(tile, "id", fileName), Frames: FramesOf(tile, fileName)))
            .OrderBy(tile => tile.Id);
        foreach (var (id, frames) in animated)
        {
            var problem = animations.ContainsKey((id % columns, id / columns)) ? $"tile {id} is animated twice." : ShapeProblemOf(id, frames, columns);
            refusal ??= problem;
            if (problem is null)
            {
                animations.Add((id % columns, id / columns), new TileAnimation { FrameCount = frames.Count, FrameMilliseconds = frames[0].Duration });
            }
        }

        return (animations, refusal);
    }

    private static List<(int TileId, int Duration)> FramesOf(XElement tile, string fileName) =>
        [.. tile.Element("animation")!.Elements("frame").Select(frame => (NumberOf(frame, "tileid", fileName), PositiveNumberOf(frame, "duration", fileName)))];

    // ADR 0039's shape: the tile's own id then the next ones, staying on its row, all for one time.
    private static string? ShapeProblemOf(int id, List<(int TileId, int Duration)> frames, int columns)
    {
        if (frames.Count < TileAnimation.MinFrames)
        {
            return $"tile {id}: an animation needs at least {TileAnimation.MinFrames} frames (has {frames.Count}).";
        }

        var isRun = frames.Select((frame, index) => frame.TileId == id + index).All(isNext => isNext);
        if (!isRun || (id % columns) + frames.Count > columns)
        {
            return $"tile {id}: frames must be the tile then the next ones on its row.";
        }

        var durations = frames.Select(frame => frame.Duration).Distinct().Order().ToList();
        return durations.Count > 1 ? $"tile {id}: frames last {string.Join(" and ", durations)} ms; one time for all is needed." : null;
    }

    private static int NumberOf(XElement element, string attribute, string fileName) =>
        int.TryParse(element.Attribute(attribute)?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var number)
            ? number
            : throw new ContentValidationException($"{fileName}: <{element.Name.LocalName}> needs a whole number {attribute}.");

    private static int PositiveNumberOf(XElement element, string attribute, string fileName) =>
        NumberOf(element, attribute, fileName) is var number && number >= 1
            ? number
            : throw new ContentValidationException($"{fileName}: <{element.Name.LocalName}> needs a {attribute} of at least 1.");

    private static int OptionalNumberOf(XElement element, string attribute, string fileName) =>
        element.Attribute(attribute) is null ? 0 : NumberOf(element, attribute, fileName);
}
