using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// classes.json is read before rules.json, so the loader checks a skill's level against the experience table only once
// it has read both files (decision 965).
public sealed class ContentCatalogLoaderClassSkillTests
{
    [Fact]
    public void Load_SkillPastMaxLevel_Refuses()
    {
        var maxLevel = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")).Rules.Experience.MaxLevel;
        using var content = new CopiedContentRoot("class-skill-level", "test-world");
        content.Replace("classes.json", "\"skillId\": \"meteor\"", $"\"skillId\": \"meteor\", \"level\": {maxLevel + 1}");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(content.Path));

        Assert.Equal(
            $"classes.json: class 'mage': skills lists 'meteor' at level {maxLevel + 1}, past the max level {maxLevel}.",
            exception.Message);
    }
}
