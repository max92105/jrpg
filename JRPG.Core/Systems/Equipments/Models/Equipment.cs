using System.Collections.Generic;

namespace JRPG.Core.Systems.Equipments.Models
{
    public abstract class Equipment
    {
        public string Id { get; set; } // unique string like "IronSword"
        public string Name { get; set; }
        public List<string> AllowedClasses { get; set; } = new List<string>();
    }
}
