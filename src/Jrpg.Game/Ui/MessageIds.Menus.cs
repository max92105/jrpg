namespace Jrpg.Game.Ui;

// The menu pages: Save and Load, Inventory, Equipment, Party, Options, the menu hints, Quests.
internal static partial class MessageIds
{
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

    public const string InventoryMaterials = "inventory_materials";

    public const string InventoryColumnItem = "inventory_column_item";

    public const string InventoryColumnQuantity = "inventory_column_quantity";

    /// <summary>Shown when the party owns nothing in the category.</summary>
    public const string InventoryEmpty = "inventory_empty";

    /// <summary>Placeholders <c>{member}</c> and <c>{amount}</c> (question 91, placeholder wording).</summary>
    public const string ItemUsedHp = "item_used_hp";

    /// <summary>Placeholders <c>{member}</c> and <c>{amount}</c>: the HP the member comes back with.</summary>
    public const string ItemUsedRevive = "item_used_revive";

    /// <summary>Placeholder <c>{item}</c>: an item used on several members at once.</summary>
    public const string ItemUsed = "item_used";

    /// <summary>The game menu's Equipment tab.</summary>
    public const string MenuEquipment = "menu_equipment";

    /// <summary>The Equipment page's action that takes off everything the member wears.</summary>
    public const string EquipmentRemoveAll = "equipment_remove_all";

    /// <summary>
    /// The comparison band's caption while a choice is open: what the highlighted item would change (E2).
    /// </summary>
    public const string EquipmentChanges = "equipment_changes";

    /// <summary>The comparison band's caption with no choice open: what the highlighted worn item gives (E2).</summary>
    public const string EquipmentGives = "equipment_gives";

    /// <summary>The comparison's last line when it has more changes than it shows: "+{count} more".</summary>
    public const string EquipmentMoreChanges = "equipment_more_changes";

    /// <summary>The equipment choice's first row, which takes off what's worn in the slot.</summary>
    public const string EquipmentRemove = "equipment_remove";

    /// <summary>The game menu's Party tab.</summary>
    public const string MenuParty = "menu_party";

    public const string PartyActive = "party_active";

    /// <summary>
    /// Placeholders <c>{count}</c> and <c>{max}</c>: how many members are active, out of how many can be.
    /// </summary>
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

    /// <summary>The Options row for the music's volume.</summary>
    public const string OptionsMusicVolume = "options_music_volume";

    /// <summary>The Options row for the sound effects' volume.</summary>
    public const string OptionsEffectsVolume = "options_effects_volume";

    /// <summary>The Options row for the voice-over's volume.</summary>
    public const string OptionsVoiceVolume = "options_voice_volume";

    public const string OptionsOn = "options_on";

    public const string OptionsOff = "options_off";

    public const string TextSpeedSlow = "text_speed_slow";

    public const string TextSpeedNormal = "text_speed_normal";

    public const string TextSpeedFast = "text_speed_fast";

    public const string TextSpeedInstant = "text_speed_instant";

    /// <summary>The menu footer's line while the player picks a tab.</summary>
    public const string MenuHintMain = "menu_hint_main";

    /// <summary>The menu footer's line on the Inventory page, while the cursor is on the list.</summary>
    public const string MenuHintInventory = "menu_hint_inventory";

    /// <summary>The menu footer's line on the Inventory page, while the cursor is on the category rail.</summary>
    public const string MenuHintInventoryCategories = "menu_hint_inventory_categories";

    /// <summary>Placeholder <c>{item}</c>: the Inventory page's footer line while choosing who gets an item.</summary>
    public const string MenuHintInventoryTarget = "menu_hint_inventory_target";

    /// <summary>The menu footer's line on the Party page.</summary>
    public const string MenuHintParty = "menu_hint_party";

    /// <summary>The menu footer's line on the Equipment page.</summary>
    public const string MenuHintEquipment = "menu_hint_equipment";

    /// <summary>The menu footer's line on the Status page.</summary>
    public const string MenuHintStatus = "menu_hint_status";

    /// <summary>The menu footer's line on the Options page.</summary>
    public const string MenuHintOptions = "menu_hint_options";

    /// <summary>The menu footer's line on the System page's own list.</summary>
    public const string MenuHintSystem = "menu_hint_system";

    /// <summary>The menu footer's line while the save files show inside System.</summary>
    public const string MenuHintSave = "menu_hint_save";

    /// <summary>The menu footer's line while the files to load show inside System.</summary>
    public const string MenuHintLoad = "menu_hint_load";

    /// <summary>
    /// Placeholders <c>{item}</c> and <c>{member}</c>: an item another member wears, in the equipment choice.
    /// </summary>
    public const string EquipmentWornRow = "equipment_worn_row";

    /// <summary>Placeholder <c>{member}</c>: the footer's line while a worn item is highlighted.</summary>
    public const string EquipmentTakeHint = "equipment_take_hint";

    /// <summary>The Quests page's name, the menu's first tab (decision 393).</summary>
    public const string MenuQuests = "menu_quests";

    /// <summary>The Quests page's footer line.</summary>
    public const string MenuHintQuests = "menu_hint_quests";

    /// <summary>The Quests page's line while no quest is active.</summary>
    public const string QuestsNone = "quests_none";

    /// <summary>A quest's kind as the page names it (decision 390).</summary>
    public const string QuestKindMain = "quest_kind_main";

    public const string QuestKindSide = "quest_kind_side";

    /// <summary>The Quests page's two categories (decision 390), named over the list.</summary>
    public const string QuestsActive = "quests_active";

    public const string QuestsCompleted = "quests_completed";

    /// <summary>The Completed category's line while no quest is completed.</summary>
    public const string QuestsNoneCompleted = "quests_none_completed";

    /// <summary>What a completed quest's detail says in place of its stage (decision 392).</summary>
    public const string QuestCompletedNote = "quest_completed_note";

    /// <summary>The band over a quest's stage text in its detail (decision 392).</summary>
    public const string QuestsObjective = "quests_objective";
}
