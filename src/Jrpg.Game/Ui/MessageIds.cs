namespace Jrpg.Game.Ui;

/// <summary>The ids of <c>content/text/ui.json</c> the game uses; a test checks each exists there (ADR 0010).</summary>
internal static class MessageIds
{
    /// <summary>Placeholder <c>{reason}</c>: why content couldn't load.</summary>
    public const string StartupFailed = "startup_failed";

    /// <summary>Placeholder <c>{item}</c>: the item's name. One copy was found.</summary>
    public const string ItemFound = "item_found";

    /// <summary>Placeholders <c>{item}</c> and <c>{count}</c>: several copies were found.</summary>
    public const string ItemFoundMany = "item_found_many";

    /// <summary>Placeholder <c>{lost}</c>: how many copies didn't fit in their full stack and were left behind.</summary>
    public const string ItemFoundLost = "item_found_lost";

    /// <summary>The game's name, written large on the title.</summary>
    public const string GameTitle = "game_title";

    /// <summary>The title's entry that starts a new game.</summary>
    public const string TitleNewGame = "title_new_game";

    /// <summary>The title's entry that continues from the most recent save.</summary>
    public const string TitleContinue = "title_continue";

    /// <summary>The title's entry that opens the save files to load one; also their heading there.</summary>
    public const string TitleLoad = "title_load";

    /// <summary>The title's entry that closes the game.</summary>
    public const string TitleQuit = "title_quit";

    /// <summary>The menu header's title while no page is open.</summary>
    public const string MenuTitle = "menu_title";

    /// <summary>The game menu's System tab.</summary>
    public const string MenuSystem = "menu_system";

    /// <summary>The System page's entry that ends the playthrough and goes back to the title.</summary>
    public const string SystemTitleScreen = "system_title_screen";

    /// <summary>The System page's entry that closes the game.</summary>
    public const string SystemQuit = "system_quit";

    /// <summary>Asked before going back to the title.</summary>
    public const string ConfirmTitleScreen = "confirm_title_screen";

    /// <summary>Asked before closing the game.</summary>
    public const string ConfirmQuit = "confirm_quit";

    /// <summary>A confirmation's button that goes ahead.</summary>
    public const string ConfirmYes = "confirm_yes";

    /// <summary>A confirmation's button that changes nothing.</summary>
    public const string ConfirmNo = "confirm_no";

    /// <summary>The game menu's Status tab.</summary>
    public const string MenuStatus = "menu_status";

    public const string StatusExperience = "status_experience";

    public const string StatusLevel = "status_level";

    public const string StatusExp = "status_exp";

    public const string StatusNextLevel = "status_next_level";

    /// <summary>Shown as the experience to the next level once a member is at the highest level.</summary>
    public const string StatusMaxLevel = "status_max_level";

    public const string StatusParameters = "status_parameters";

    public const string StatusEquipment = "status_equipment";

    public const string StatHp = "stat_hp";

    public const string StatMp = "stat_mp";

    /// <summary>Placeholders <c>{value}</c> and <c>{max}</c>: an HP or MP value, the one format everywhere (decision 110).</summary>
    public const string StatValueOfMax = "stat_value_of_max";

    public const string StatAttack = "stat_attack";

    public const string StatDefense = "stat_defense";

    public const string SlotWeapon = "slot_weapon";

    public const string SlotArmor = "slot_armor";

    /// <summary>Shown for an equipment slot with nothing worn.</summary>
    public const string SlotEmpty = "slot_empty";

    /// <summary>The System page's entry that opens the save files.</summary>
    public const string SystemSave = "system_save";

    /// <summary>The System page's entry that opens the save files to load one.</summary>
    public const string SystemLoad = "system_load";

    public const string SaveColumnFile = "save_column_file";

    public const string SaveColumnLocation = "save_column_location";

    public const string SaveColumnParty = "save_column_party";

    /// <summary>Placeholder <c>{number}</c>: the file's number, 1 to 20.</summary>
    public const string SaveFile = "save_file";

    public const string SaveEmpty = "save_empty";

    /// <summary>A save file that exists but can't be loaded.</summary>
    public const string SaveDamaged = "save_damaged";

    /// <summary>Placeholders <c>{date}</c> and <c>{time}</c>: when the save was made, and its play time.</summary>
    public const string SaveDetails = "save_details";

    /// <summary>Placeholders <c>{name}</c> and <c>{level}</c>: one active member in a save file's entry.</summary>
    public const string SaveMember = "save_member";

    /// <summary>Placeholder <c>{number}</c>: asked before saving onto a file in use.</summary>
    public const string SaveOverwrite = "save_overwrite";

    /// <summary>Placeholder <c>{number}</c>: the disk refused the save.</summary>
    public const string SaveFailed = "save_failed";

    /// <summary>The game menu's Inventory tab.</summary>
    public const string MenuInventory = "menu_inventory";

    /// <summary>The inventory heading when every item shows.</summary>
    public const string InventoryAll = "inventory_all";

    public const string InventoryConsumables = "inventory_consumables";

    public const string InventoryWeapons = "inventory_weapons";

    public const string InventoryArmor = "inventory_armor";

    public const string InventoryColumnName = "inventory_column_name";

    public const string InventoryColumnQuantity = "inventory_column_quantity";

    /// <summary>Shown when the party owns nothing in the category.</summary>
    public const string InventoryEmpty = "inventory_empty";

    /// <summary>Placeholder <c>{item}</c>: the heading over the item target picker.</summary>
    public const string ItemUseTitle = "item_use_title";

    /// <summary>Placeholders <c>{member}</c> and <c>{amount}</c> (question 91, placeholder wording).</summary>
    public const string ItemUsedHp = "item_used_hp";

    /// <summary>Placeholders <c>{member}</c> and <c>{amount}</c>.</summary>
    public const string ItemUsedMp = "item_used_mp";

    /// <summary>Placeholders <c>{member}</c> and <c>{amount}</c>: the HP the member comes back with.</summary>
    public const string ItemUsedRevive = "item_used_revive";

    /// <summary>Placeholder <c>{item}</c>: an item used on several members at once.</summary>
    public const string ItemUsed = "item_used";

    /// <summary>The game menu's Equipment tab.</summary>
    public const string MenuEquipment = "menu_equipment";

    /// <summary>The Equipment page's action that takes off everything the member wears.</summary>
    public const string EquipmentRemoveAll = "equipment_remove_all";

    public const string StatMaxHp = "stat_max_hp";

    public const string StatMaxMp = "stat_max_mp";

    /// <summary>The equipment choice's first row, which takes off what's worn in the slot.</summary>
    public const string EquipmentRemove = "equipment_remove";

    /// <summary>Placeholder <c>{item}</c>: the worn item that can't go back in its full inventory stack.</summary>
    public const string EquipmentNoRoom = "equipment_no_room";

    /// <summary>The game menu's Party tab.</summary>
    public const string MenuParty = "menu_party";

    public const string PartyActive = "party_active";

    /// <summary>Placeholders <c>{count}</c> and <c>{max}</c>: how many members are active, out of how many can be.</summary>
    public const string PartyCount = "party_count";

    public const string PartyReserve = "party_reserve";

    /// <summary>The badge beside the leader's name.</summary>
    public const string PartyLeader = "party_leader";

    /// <summary>Placeholder <c>{level}</c>: a member card's level.</summary>
    public const string PartyLevel = "party_level";

    /// <summary>Shown when a reserve member can't join because the active party has no free place.</summary>
    public const string PartyFull = "party_full";

    /// <summary>Placeholder <c>{name}</c>: shown when the leader is chosen, who can't leave the party.</summary>
    public const string PartyLeaderStays = "party_leader_stays";

    /// <summary>Placeholder <c>{name}</c>: shown when the only active member standing is sent to the reserve.</summary>
    public const string PartyNeedsSomeoneStanding = "party_needs_someone_standing";

    /// <summary>The active member's choice that starts swapping their place with another's.</summary>
    public const string PartyChangeOrder = "party_change_order";

    /// <summary>The active member's choice that sends them to the reserve.</summary>
    public const string PartyToReserve = "party_to_reserve";

    /// <summary>The game menu's Options tab.</summary>
    public const string MenuOptions = "menu_options";

    public const string OptionsFullScreen = "options_full_screen";

    public const string OptionsTextSpeed = "options_text_speed";

    public const string OptionsOn = "options_on";

    public const string OptionsOff = "options_off";

    public const string TextSpeedSlow = "text_speed_slow";

    public const string TextSpeedNormal = "text_speed_normal";

    public const string TextSpeedFast = "text_speed_fast";

    public const string TextSpeedInstant = "text_speed_instant";

    /// <summary>The battle command that hits one enemy.</summary>
    public const string BattleAttack = "battle_attack";

    /// <summary>The battle command that opens the actor's skills.</summary>
    public const string BattleSkills = "battle_skills";

    /// <summary>The battle command that opens the party's items.</summary>
    public const string BattleItems = "battle_items";

    /// <summary>The battle command that passes the turn on guard.</summary>
    public const string BattleDefend = "battle_defend";

    /// <summary>The battle command that leaves the fight.</summary>
    public const string BattleRun = "battle_run";

    /// <summary>The hint under the command panel while Attack is selected.</summary>
    public const string BattleAttackHint = "battle_attack_hint";

    /// <summary>The hint under the command panel while Skills is selected.</summary>
    public const string BattleSkillsHint = "battle_skills_hint";

    /// <summary>The hint under the command panel while Items is selected.</summary>
    public const string BattleItemsHint = "battle_items_hint";

    /// <summary>The hint under the command panel while Defend is selected.</summary>
    public const string BattleDefendHint = "battle_defend_hint";

    /// <summary>The hint under the command panel while Run is selected.</summary>
    public const string BattleRunHint = "battle_run_hint";

    /// <summary>Placeholder <c>{max}</c>: a vital's maximum, written small after its value on the battle screen.</summary>
    public const string BattleVitalMax = "battle_vital_max";

    /// <summary>Placeholders <c>{name}</c> and <c>{letter}</c>: an enemy sharing its name with others, as "Goblin A".</summary>
    public const string BattleEnemyLetter = "battle_enemy_letter";

    /// <summary>The label over the turn order's upcoming turns.</summary>
    public const string BattleNext = "battle_next";

    /// <summary>Placeholder <c>{name}</c>: the banner when a combatant defends.</summary>
    public const string BattleDefends = "battle_defends";

    /// <summary>Placeholder <c>{cost}</c>: a skill's MP cost on its row in battle.</summary>
    public const string BattleSkillCost = "battle_skill_cost";

    /// <summary>The hint when Confirm is pressed on a skill the actor lacks the MP for (decision 105).</summary>
    public const string BattleNotEnoughMp = "battle_not_enough_mp";

    /// <summary>Placeholder <c>{count}</c>: how many copies of an item the party owns, on its row in battle.</summary>
    public const string BattleItemCount = "battle_item_count";

    /// <summary>The title of the victory screen.</summary>
    public const string BattleVictory = "battle_victory";

    /// <summary>Placeholder <c>{amount}</c>: the experience the defeated enemies gave, before the split.</summary>
    public const string BattleExperienceEarned = "battle_experience_earned";

    /// <summary>Shown on the victory screen by a member whose bar filled a level.</summary>
    public const string BattleLevelUp = "battle_level_up";
}
