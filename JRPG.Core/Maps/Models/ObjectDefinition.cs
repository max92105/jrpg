using System.Text.Json.Serialization;
using System.Windows;

namespace JRPG.Core.Maps.Models
{
    public class ObjectDefinition
    {
		public string SpritePath { get; set; }
		public bool HasCollision { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public Rect? CollisionBox { get; set; }
	}
}
