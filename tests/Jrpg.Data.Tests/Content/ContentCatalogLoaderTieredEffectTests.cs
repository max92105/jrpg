using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// effects.json loads first and a skill's effect names a record by id, resolved as skills.json is read (decision 789),
// on a copy of the repository's content with Fire Blast's tier changed here: the one skill naming fire_1 and granting a
// status after it.
public sealed class ContentCatalogLoaderTieredEffectTests : IDisposable
{
    private const string FireBlastTier = "\"tier\": \"fire_1\",\r\n      \"statuses\"";

    private readonly CopiedContentRoot _content = new("tiered-effects", "content");

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Load_SkillNamingATieredEffect_HoldsTheRecord()
    {
        NameTier("fire_2");

        var catalog = ContentCatalogLoader.Load(_content.Path);

        Assert.Same(catalog.TieredEffects["fire_2"], catalog.Skills["fire_blast"].Effect.Tier);
    }

    [Fact]
    public void Load_SkillNamingAnUnknownTieredEffect_RefusesNamingFileAndId()
    {
        NameTier("fire_9");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.StartsWith("skills.json", exception.Message, StringComparison.Ordinal);
        Assert.Contains("fire_9", exception.Message, StringComparison.Ordinal);
    }

    // Ids only: the numbers are the developer's to tune.
    [Fact]
    public void Load_RepositoryEffects_HoldFire1()
    {
        var fire1 = ContentCatalogLoader.Load(_content.Path).TieredEffects["fire_1"];

        Assert.Equal((EffectKind.Damage, DamageType.Fire), (fire1.Kind, fire1.DamageType));
    }

    // The load runs effects.json's own rules, not only its shape.
    [Fact]
    public void Load_EffectWithPerPowerZero_RefusesNamingTheRecord()
    {
        const string Fire1 = "\"id\": \"fire_1\",\r\n    \"kind\": \"damage\",\r\n    \"perPower\": ";
        _content.Replace("effects.json", Fire1 + "2", Fire1 + "0");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.Equal("effects.json: effect 'fire_1': perPower must be within 1..1000 (was 0).", exception.Message);
    }

    [Fact]
    public void Load_EffectsFileMissing_RefusesNamingIt()
    {
        File.Delete(_content.PathOf("effects.json"));

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.Equal("effects.json: the file is missing.", exception.Message);
    }

    private void NameTier(string id) =>
        _content.Replace("skills.json", FireBlastTier, $"\"tier\": \"{id}\",\r\n      \"statuses\"");
}
