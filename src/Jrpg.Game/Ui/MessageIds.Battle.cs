namespace Jrpg.Game.Ui;

// The battle screen, its statuses and its outcomes.
internal static partial class MessageIds
{
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

    /// <summary>
    /// Placeholder <c>{max}</c>: a vital's maximum, written small after its value on the battle screen.
    /// </summary>
    public const string BattleVitalMax = "battle_vital_max";

    /// <summary>
    /// Placeholders <c>{name}</c> and <c>{letter}</c>: an enemy sharing its name with others, as "Goblin A".
    /// </summary>
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

    /// <summary>The cue on an enemy entering a phase (decision 406).</summary>
    public const string BattleEnemyPhase = "battle_enemy_phase";

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

    /// <summary>The victory screen's reward row for the gold the battle gave (decision 103).</summary>
    public const string BattleVictoryGold = "battle_victory_gold";

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
}
