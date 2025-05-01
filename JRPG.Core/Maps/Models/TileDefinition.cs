namespace JRPG.Core.Models
{
    public class TileDefinition
    {
        public string Name { get; set; }
        public bool IsWalkable { get; set; }

        public string Sprite { get; set; }
        public string[] Frames { get; set; }
        public double FrameDuration { get; set; }
    }
}
