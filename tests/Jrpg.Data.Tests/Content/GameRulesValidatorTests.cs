using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

public sealed class GameRulesValidatorTests
{
    [Fact]
    public void Validate_ZeroMaxActiveMembers_ThrowsNamingTheField()
    {
        var rules = new GameRules(new PartyRules(MaxActiveMembers: 0), new ExperienceTable([100]));

        var exception = Assert.Throws<ContentValidationException>(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.Equal("rules.json: party.maxActiveMembers must be at least 1 (was 0).", exception.Message);
    }

    [Fact]
    public void Validate_ZeroExperienceEntry_ThrowsNamingTheIndex()
    {
        var rules = new GameRules(new PartyRules(MaxActiveMembers: 4), new ExperienceTable([100, 0]));

        var exception = Assert.Throws<ContentValidationException>(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.Equal("rules.json: experience.experienceToNextLevel[1] must be at least 1 (was 0).", exception.Message);
    }
}
