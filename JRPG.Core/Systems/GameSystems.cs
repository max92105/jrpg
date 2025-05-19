using JRPG.Core.Battles;
using JRPG.Core.Battles.Models;
using JRPG.Core.Systems.Inventories;
using JRPG.Core.Systems.Parties;
using System.Collections.Generic;

namespace JRPG.Core.Systems
{
    public static class GameSystems
    {
        public static InventoryManager Inventory { get; private set; }
        public static PartyManager Party { get; private set; }
        public static EquipmentManager Equipment { get; private set; }
        public static BattleManager Battle { get; private set; }

        public static void Initialize()
        {
            Inventory = new InventoryManager();
            Party = new PartyManager();
            Equipment = new EquipmentManager();
        }

        public static void SetBattleManager(List<BattleActor> battleActors)
        {
            Battle = new BattleManager(battleActors);
        }
    }
}