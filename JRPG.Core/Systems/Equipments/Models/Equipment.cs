using JRPG.Core.Systems.Inventories.Models;
using System.Collections.Generic;

namespace JRPG.Core.Systems.Equipments.Models
{
    public abstract class Equipment: Item
    {
        public List<string> AllowedClasses { get; set; } = new List<string>();  

        protected Equipment()
        {
            CanBeUsedFromInventory = false;
        }
    }
}
