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

    /// <summary>Placeholder <c>{name}</c>: a recruit step added that member to the party.</summary>
    public const string MemberJoined = "member_joined";

    /// <summary>A rest step rested the whole party (decision 230).</summary>
    public const string PartyRests = "event_party_rests";

    /// <summary>Placeholder <c>{name}</c>: a recruit step named a member already in the party.</summary>
    public const string MemberAlreadyInParty = "member_already_in_party";

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

    /// <summary>The line at the bottom of the title screen.</summary>
    public const string TitleFooter = "title_footer";

    /// <summary>The heading of the screen where a new game's leader is chosen.</summary>
    public const string LeaderChoiceTitle = "leader_choice_title";

    /// <summary>Placeholder <c>{name}</c>: asked before a new game starts led by that character.</summary>
    public const string LeaderChoiceConfirm = "leader_choice_confirm";

    /// <summary>The leader choice's footer hint.</summary>
    public const string LeaderChoiceHint = "leader_choice_hint";

    /// <summary>Placeholder <c>{button}</c>: the Confirm button's name; shown while cutscene stills play.</summary>
    public const string CinematicSkip = "cinematic_skip";

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

    /// <summary>A notice dialog's one button, which closes it.</summary>
    public const string NoticeOk = "notice_ok";

    /// <summary>The game menu's Status tab.</summary>
    public const string MenuStatus = "menu_status";

    public const string StatusLevel = "status_level";

    /// <summary>The level's short caption, "Lv", beside a number.</summary>
    public const string StatusLevelShort = "status_level_short";

    public const string StatusExp = "status_exp";

    public const string StatusNextLevel = "status_next_level";

    /// <summary>Shown as the experience to the next level once a member is at the highest level.</summary>
    public const string StatusMaxLevel = "status_max_level";

    /// <summary>The Status page's section of the five primaries, beside the profile.</summary>
    public const string StatusAttributes = "status_attributes";

    /// <summary>What Power gives, beside it, each value signed: "{damage} % damage".</summary>
    public const string StatusPowerGives = "status_power_gives";

    /// <summary>What Agility gives: "{crit} % crit, {dodge} % dodge".</summary>
    public const string StatusAgilityGives = "status_agility_gives";

    /// <summary>What Intelligence gives: "{status} % status".</summary>
    public const string StatusIntelligenceGives = "status_intelligence_gives";

    /// <summary>What Constitution gives: "{hp} % HP, {resist} % phys. res.".</summary>
    public const string StatusConstitutionGives = "status_constitution_gives";

    /// <summary>What Will gives: "{resist} % magic res.".</summary>
    public const string StatusWillGives = "status_will_gives";

    /// <summary>
    /// Placeholders <c>{points}</c> and <c>{resource}</c>: what Will adds to one class resource (decision 319).
    /// </summary>
    public const string StatusWillGivesResource = "status_will_gives_resource";

    /// <summary>What joins the parts of an attribute caption.</summary>
    public const string StatusCaptionSeparator = "status_caption_separator";

    /// <summary>The Status page's section of the numbers a fight uses.</summary>
    public const string StatusCombat = "status_combat";

    /// <summary>The Status page's section of the eight resistances.</summary>
    public const string StatusResistances = "status_resistances";

    /// <summary>The Status page's crit line: chance and damage together.</summary>
    public const string StatusCrit = "status_crit";

    /// <summary>The crit line's value: "{chance} · {damage}".</summary>
    public const string StatusCritValue = "status_crit_value";

    /// <summary>The Status page's short label for crowd control resistance.</summary>
    public const string StatusCrowdControl = "status_crowd_control";

    public const string DamageTypeBlunt = "damage_type_blunt";

    public const string DamageTypePiercing = "damage_type_piercing";

    public const string DamageTypeSlashing = "damage_type_slashing";

    public const string DamageTypeFire = "damage_type_fire";

    public const string DamageTypeIce = "damage_type_ice";

    public const string DamageTypeLightning = "damage_type_lightning";

    public const string DamageTypeLight = "damage_type_light";

    public const string DamageTypeDark = "damage_type_dark";

    public const string StatusEquipment = "status_equipment";

    public const string StatusSpeed = "status_speed";

    public const string StatusResources = "status_resources";

    public const string StatusAffixes = "status_affixes";

    /// <summary>Armor penetration's short label on the Status page, which fits its cell.</summary>
    public const string StatusPenetration = "status_penetration";

    public const string StatusSkills = "status_skills";

    /// <summary>A lasting resource's amount out of its max: {amount}, {max}.</summary>
    public const string StatusLastingAmount = "status_lasting_amount";

    public const string StatHp = "stat_hp";

    /// <summary>Placeholders <c>{value}</c>, <c>{max}</c>: an HP value, one format everywhere (decision 110).</summary>
    public const string StatValueOfMax = "stat_value_of_max";

    /// <summary>A basic attack: the weapon's damage or the unarmed one, raised by Power (decision 337).</summary>
    public const string StatWeaponDamage = "stat_weapon_damage";

    public const string StatPower = "stat_power";

    public const string StatAgility = "stat_agility";

    public const string StatIntelligence = "stat_intelligence";

    public const string StatConstitution = "stat_constitution";

    public const string StatWill = "stat_will";

    public const string StatAccuracy = "stat_accuracy";

    public const string StatCritChance = "stat_crit_chance";

    public const string StatCritDamage = "stat_crit_damage";

    public const string StatDodge = "stat_dodge";

    public const string StatCrowdControlResistance = "stat_crowd_control_resistance";

    public const string StatBluntResistance = "stat_blunt_resistance";

    public const string StatPiercingResistance = "stat_piercing_resistance";

    public const string StatSlashingResistance = "stat_slashing_resistance";

    public const string StatFireResistance = "stat_fire_resistance";

    public const string StatIceResistance = "stat_ice_resistance";

    public const string StatLightningResistance = "stat_lightning_resistance";

    public const string StatLightResistance = "stat_light_resistance";

    public const string StatDarkResistance = "stat_dark_resistance";

    public const string StatLifeSteal = "stat_life_steal";

    public const string StatArmorPenetration = "stat_armor_penetration";

    public const string StatBlockChance = "stat_block_chance";

    public const string StatDropLuck = "stat_drop_luck";

    /// <summary>A percent stat's value: "{value} %".</summary>
    public const string StatPercent = "stat_percent";

    /// <summary>
    /// Placeholders <c>{value}</c> (signed, a percent with its sign) and <c>{stat}</c>: one rolled affix.
    /// </summary>
    public const string AffixLine = "affix_line";

    /// <summary>Between a rolled piece's rarity and each of its affixes on one line.</summary>
    public const string AffixSeparator = "affix_separator";

    public const string RarityCommon = "rarity_common";

    public const string RarityMagic = "rarity_magic";

    public const string RarityRare = "rarity_rare";

    public const string RarityUnique = "rarity_unique";

    public const string SlotWeapon = "slot_weapon";

    public const string SlotChest = "slot_chest";

    public const string SlotOffHand = "slot_off_hand";

    public const string SlotHead = "slot_head";

    public const string SlotHands = "slot_hands";

    public const string SlotFeet = "slot_feet";

    public const string SlotAmulet = "slot_amulet";

    /// <summary>Both ring slots' label (ADR 0028).</summary>
    public const string SlotRing = "slot_ring";

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

    /// <summary>The battle command that trades places with an ally right beside the actor (ADR 0023).</summary>
    public const string BattleMove = "battle_move";

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

    /// <summary>The hint under the command panel while Move is selected.</summary>
    public const string BattleMoveHint = "battle_move_hint";

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

    /// <summary>Placeholders <c>{amount}</c>, <c>{resource}</c>: a skill's resource cost on its battle row.</summary>
    public const string BattleSkillResourceCost = "battle_skill_resource_cost";

    /// <summary>Placeholders <c>{amount}</c>, <c>{resource}</c>: what a skill adds, on its battle row.</summary>
    public const string BattleSkillResourceGain = "battle_skill_resource_gain";

    /// <summary>Placeholders <c>{change}</c>, <c>{need}</c>: a skill row's change and requirement together.</summary>
    public const string BattleSkillChangeAndNeed = "battle_skill_change_and_need";

    /// <summary>Placeholders <c>{amount}</c>, <c>{resource}</c>: an unmet requirement on its battle row.</summary>
    public const string BattleSkillResourceNeeded = "battle_skill_resource_needed";

    /// <summary>Placeholders <c>{amount}</c>, <c>{resource}</c>: a met requirement on its battle row.</summary>
    public const string BattleSkillResourceLevel = "battle_skill_resource_level";

    /// <summary>Placeholder <c>{resource}</c>: the hint on Confirm when the actor lacks it for the skill.</summary>
    public const string BattleNotEnoughResource = "battle_not_enough_resource";

    /// <summary>The Stun status's name: its cue when granted, and its tag (ADR 0021).</summary>
    public const string StatusStun = "status_stun";

    /// <summary>The Taunt status's name: its cue when granted, and its tag (ADR 0021).</summary>
    public const string StatusTaunt = "status_taunt";

    /// <summary>The Guarded status's name: its cue when granted, and its tag (ADR 0021).</summary>
    public const string StatusGuarded = "status_guarded";

    /// <summary>The Exposed status's name: its cue when granted, and its tag (ADR 0021).</summary>
    public const string StatusExposed = "status_exposed";

    /// <summary>The Poison status's name: its cue when granted, and its tag (ADR 0021).</summary>
    public const string StatusPoison = "status_poison";

    /// <summary>The Regen status's name: its cue when granted, and its tag (ADR 0021).</summary>
    public const string StatusRegen = "status_regen";

    /// <summary>The cue on a combatant whose turn a stun took.</summary>
    public const string BattleTurnSkipped = "battle_turn_skipped";

    /// <summary>The cue on a caster whose spell turned on them (decision 194).</summary>
    public const string BattleBackfire = "battle_backfire";

    /// <summary>The cue on a target a damaging hit missed (decision 326).</summary>
    public const string BattleMiss = "battle_miss";

    /// <summary>The cue on a target that blocked a landed hit (decision 348).</summary>
    public const string BattleBlocked = "battle_blocked";

    /// <summary>The cue by a hit a weakness raised (decision 323).</summary>
    public const string BattleWeak = "battle_weak";

    /// <summary>
    /// The cue by a hit a resistance big enough to count lowered (decision 323), and on a target that shrugged off a
    /// stun (decision 324).
    /// </summary>
    public const string BattleResisted = "battle_resisted";

    /// <summary>Placeholder <c>{count}</c>: how many copies of an item the party owns, on its row in battle.</summary>
    public const string BattleItemCount = "battle_item_count";

    /// <summary>The title of the victory screen.</summary>
    public const string BattleVictory = "battle_victory";

    /// <summary>The small label before a member's level on a victory card.</summary>
    public const string BattleLevelLabel = "battle_level_label";

    /// <summary>The victory screen's footer hint once everything has filled: Confirm leaves.</summary>
    public const string BattleVictoryNext = "battle_victory_next";

    /// <summary>The victory screen's heading over what dropped (decision 355).</summary>
    public const string BattleVictoryItems = "battle_victory_items";

    /// <summary>
    /// Placeholder <c>{count}</c>: how many copies of a stack dropped, after its name on the Items step.
    /// </summary>
    public const string BattleVictoryDropCount = "battle_victory_drop_count";

    /// <summary>Placeholder <c>{count}</c>: the drops past what the Items step has room for.</summary>
    public const string BattleVictoryMoreDrops = "battle_victory_more_drops";

    /// <summary>The banner after a successful Run.</summary>
    public const string BattleEscaped = "battle_escaped";

    /// <summary>The defeat screen's title.</summary>
    public const string BattleDefeat = "battle_defeat";

    /// <summary>The line under the defeat screen's title.</summary>
    public const string BattlePartyFallen = "battle_party_fallen";

    /// <summary>The defeat choice that loads the most recent save at once.</summary>
    public const string BattleLoadLastSave = "battle_load_last_save";

    /// <summary>The defeat choice that ends the session for the title screen.</summary>
    public const string BattleMainMenu = "battle_main_menu";

    /// <summary>The defeat screen's footer hint.</summary>
    public const string BattleDefeatHint = "battle_defeat_hint";

    /// <summary>Shown on the victory screen by a member whose bar filled a level.</summary>
    public const string BattleLevelUp = "battle_level_up";

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

    /// <summary>Placeholders <c>{item}</c> and <c>{member}</c>: an item another member wears, in the equipment choice.</summary>
    public const string EquipmentWornRow = "equipment_worn_row";

    /// <summary>Placeholder <c>{member}</c>: the footer's line while a worn item is highlighted.</summary>
    public const string EquipmentTakeHint = "equipment_take_hint";
}
