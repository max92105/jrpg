using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// Each skill names its sound (decision 423), checked by the entries the tests need, not by a total.
public sealed class ContentCatalogLoaderSkillSoundTests
{
    [Theory]
    [InlineData("fire_blast", "PlaceholderSkillFire.wav")]
    [InlineData("heal", "PlaceholderSkillMend.wav")]
    [InlineData("taunt", "PlaceholderSkillShout.wav")]
    public void Load_RepositorySkills_NameTheirSounds(string skillId, string soundFile)
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        Assert.Equal(soundFile, catalog.Skills[skillId].SoundFile);
    }

    [Fact]
    public void Load_SkillWithoutASound_ThrowsNamingTheSkillAndField()
    {
        using var content = new CopiedContentRoot("skill-sound");
        content.Replace("skills.json", "\"soundFile\": \"PlaceholderSkillFire.wav\",", "");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(content.Path));

        Assert.Contains("soundFile must name a sound.", exception.Message, StringComparison.Ordinal);
        Assert.Contains("skill 'fire_blast'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("skills.json", exception.Message, StringComparison.Ordinal);
    }
}
