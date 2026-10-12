using JRPG.Core.Battles;
using JRPG.Core.Battles.Models;
using JRPG.Core.Systems.Inventories;
using JRPG.Core.Systems.Parties;
using JRPG.Core.Systems.SaveGames;
using System.Collections.Generic;

namespace JRPG.Core.Systems
{
    public static class GameSystems
    {
        private static bool isInitialized = false;

        public static InventoryManager Inventory { get; private set; }
        public static PartyManager Party { get; private set; }
        public static EquipmentManager Equipment { get; private set; }
        public static GameSystemManager System { get; private set; }
        public static SaveGameManager Save { get; private set; }
        public static BattleManager Battle { get; private set; }
        public static WorldState World { get; private set; }

        public static void Initialize()
        {
            if (isInitialized)
                return;

            World = new WorldState();
            Inventory = new InventoryManager();
            Party = new PartyManager();
            Equipment = new EquipmentManager();
            System = new GameSystemManager();
            Save = new SaveGameManager();

            System.StartSessionTimer();

            isInitialized = true;
        }

        public static void Reset()
        {
            isInitialized = false;
            Initialize();
        }

        public static void SetBattleManager(List<BattleActor> battleActors)
        {
            Battle = new BattleManager(battleActors);
        }
    }
}
