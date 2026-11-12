namespace Jrpg.Game.Ui;

// Names several screens share: damage types, stats, affixes, rarities, slots.
internal static partial class MessageIds
{
    public const string DamageTypeBlunt = "damage_type_blunt";

    public const string DamageTypePiercing = "damage_type_piercing";

    public const string DamageTypeSlashing = "damage_type_slashing";

    public const string DamageTypeFire = "damage_type_fire";

    public const string DamageTypeIce = "damage_type_ice";

    public const string DamageTypeLightning = "damage_type_lightning";

    public const string DamageTypeLight = "damage_type_light";

    public const string DamageTypeDark = "damage_type_dark";

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
}
