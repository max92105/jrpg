using Jrpg.Core.Content;

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
}
