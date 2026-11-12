namespace Jrpg.Game.Ui;

// The Status page's labels.
internal static partial class MessageIds
{
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

    public const string StatusEquipment = "status_equipment";

    public const string StatusSpeed = "status_speed";

    public const string StatusResources = "status_resources";

    public const string StatusAffixes = "status_affixes";

    /// <summary>Armor penetration's short label on the Status page, which fits its cell.</summary>
    public const string StatusPenetration = "status_penetration";

    public const string StatusSkills = "status_skills";

    /// <summary>A lasting resource's amount out of its max: {amount}, {max}.</summary>
    public const string StatusLastingAmount = "status_lasting_amount";
}
