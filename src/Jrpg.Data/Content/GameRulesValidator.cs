using System.Globalization;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Stats;

namespace Jrpg.Data.Content;

internal static class GameRulesValidator
{
    public static void Validate(string fileName, GameRules rules)
    {
        if (rules.Party.MaxActiveMembers < 1)
        {
            throw new ContentValidationException(
                $"{fileName}: party.maxActiveMembers must be at least 1 (was {rules.Party.MaxActiveMembers}).");
        }

        if (rules.Party.MaxActiveMembers > PartyRules.MostActiveMembers)
        {
            throw new ContentValidationException(
                $"{fileName}: party.maxActiveMembers can be at most {PartyRules.MostActiveMembers} (was {rules.Party.MaxActiveMembers}).");
        }

        ValidateExploration(fileName, rules.Exploration);

        ValidateStats(fileName, rules.Stats);

        var thresholds = rules.Experience.ExperienceToNextLevel;
        if (thresholds.Count == 0)
        {
            // An empty table caps everyone at level 1, so nobody could ever level up (developer decision 24).
            throw new ContentValidationException($"{fileName}: experience.experienceToNextLevel must have at least one entry.");
        }

        for (var index = 0; index < thresholds.Count; index++)
        {
            if (thresholds[index] < 1)
            {
                throw new ContentValidationException(
                    $"{fileName}: experience.experienceToNextLevel[{index}] must be at least 1 (was {thresholds[index]}).");
            }
        }
    }

    // A negative rate would take something away for every point, and a negative base starts below nothing.
    private static void ValidateStats(string fileName, StatRules stats)
    {
        (string Field, int Value)[] fields =
        [
            ("hpPercentPer100Constitution", stats.HpPercentPer100Constitution),
            ("baseAccuracy", stats.BaseAccuracy),
            ("baseCritChance", stats.BaseCritChance),
            ("baseDodge", stats.BaseDodge),
            ("baseCrowdControlResistance", stats.BaseCrowdControlResistance),
            ("damagePercentPer100Power", stats.DamagePercentPer100Power),
            ("critDamagePercentPer100Power", stats.CritDamagePercentPer100Power),
            ("critChancePer100Agility", stats.CritChancePer100Agility),
            ("dodgePer100Agility", stats.DodgePer100Agility),
            ("physicalResistancePer100Constitution", stats.PhysicalResistancePer100Constitution),
            ("magicalResistancePer100Will", stats.MagicalResistancePer100Will),
            ("statusPercentPer100Intelligence", stats.StatusPercentPer100Intelligence),
        ];
        var (negativeField, value) = fields.FirstOrDefault(field => field.Value < 0);
        if (negativeField is not null)
        {
            throw new ContentValidationException($"{fileName}: stats.{negativeField} must be at least 0 (was {value}).");
        }

        // The breakpoint divides Intelligence: 0 would divide by zero, and a negative one take turns away.
        if (stats.IntelligencePerExtraStatusTurn < 1)
        {
            throw new ContentValidationException(
                $"{fileName}: stats.intelligencePerExtraStatusTurn must be at least 1 (was {stats.IntelligencePerExtraStatusTurn}).");
        }

        // An unarmed hit for nothing would make a member without a weapon unable to hurt anyone.
        if (stats.UnarmedDamage < 1)
        {
            throw new ContentValidationException($"{fileName}: stats.unarmedDamage must be at least 1 (was {stats.UnarmedDamage}).");
        }
    }

    private static void ValidateExploration(string fileName, ExplorationRules exploration)
    {
        if (!double.IsFinite(exploration.WalkSpeed) || exploration.WalkSpeed <= 0)
        {
            throw new ContentValidationException(
                string.Create(CultureInfo.InvariantCulture, $"{fileName}: exploration.walkSpeed must be above 0 (was {exploration.WalkSpeed})."));
        }

        // The body must fit inside one cell, or no map cell could ever hold the player.
        if (!double.IsFinite(exploration.PlayerRadius) || exploration.PlayerRadius is <= 0 or >= 0.5)
        {
            throw new ContentValidationException(
                string.Create(CultureInfo.InvariantCulture, $"{fileName}: exploration.playerRadius must be above 0 and below 0.5 (was {exploration.PlayerRadius})."));
        }

        if (!double.IsFinite(exploration.InteractReach) || exploration.InteractReach <= 0)
        {
            throw new ContentValidationException(
                string.Create(CultureInfo.InvariantCulture, $"{fileName}: exploration.interactReach must be above 0 (was {exploration.InteractReach})."));
        }
    }
}
