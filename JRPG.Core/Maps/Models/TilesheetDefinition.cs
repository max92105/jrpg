namespace JRPG.Core.Maps.Models
{
    /// <summary>
    /// How a tilesheet image is cut into tiles. Tiles are always drawn at
    /// MapConstant.TileSize on the map, whatever their size in the sheet.
    /// </summary>
    public class TilesheetDefinition
    {
        public string File { get; set; }        // image file name in Assets/Tiles
        public string Name { get; set; }        // display name
        public int TileWidth { get; set; } = MapConstant.TileSize;
        public int TileHeight { get; set; } = MapConstant.TileSize;
        public int Margin { get; set; }         // empty pixels around the whole sheet
        public int Spacing { get; set; }        // empty pixels between tiles

        public int ColumnsFor(int imageWidth) =>
            TileWidth <= 0 ? 0 : (imageWidth - 2 * Margin + Spacing) / (TileWidth + Spacing);

        public int RowsFor(int imageHeight) =>
            TileHeight <= 0 ? 0 : (imageHeight - 2 * Margin + Spacing) / (TileHeight + Spacing);

        public int CellLeft(int column) => Margin + column * (TileWidth + Spacing);
        public int CellTop(int row) => Margin + row * (TileHeight + Spacing);
    }
}
