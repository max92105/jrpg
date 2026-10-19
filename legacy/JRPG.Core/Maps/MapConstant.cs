namespace JRPG.Core.Maps
{
    public static class MapConstant
    {
        public const int TileSize = 48;

        // Draw layers: lower Z is drawn first. Things at PlayerZ are depth-sorted with
        // the player by their bottom edge; AbovePlayerZ is always drawn over the player.
        public const int PlayerZ = 1;
        public const int AbovePlayerZ = 5;
        public const string TilesetJsonPath = "Tiles\\tileset.json";
        public const string ObjectSetJsonPath = "Objects\\objects.json";
        public const string MapFolderPath = "Maps\\";
    }
}
