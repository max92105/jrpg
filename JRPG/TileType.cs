using JRPG.Enums;

namespace JRPG
{
    public class TileType
    {
        public string Name { get; set; }
        public bool IsWalkable { get; set; }
        public string Sprite { get; set; }

        public TileTriggerType TriggerType { get; set; } = TileTriggerType.None;

        public string TriggerData { get; set; } // e.g., "Maps/house.map:1,1"
    }
}
