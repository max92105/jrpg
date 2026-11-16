using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Editor.Skills;

namespace Jrpg.Editor.Tests.Skills;

// The Skills page's read-only lines for the chosen skill: who uses it (decision 978) and its strip's pace from the
// draft's records (decision 982), each following the choice and the strip box.
public sealed class SkillCatalogLinesTests
{
    [Fact]
    public void StripPaceText_ListedStrip_ReadsItsPaceAndHit()
    {
        var pace = new EffectStripDefinition { File = "BoltStrip.png", FrameMilliseconds = 60, HitFrame = 2 };
        var choices = SkillFixtures.NoChoices with { StripPaces = ById(pace.File, pace) };

        var catalog = new SkillCatalogViewModel(Catalog(), choices);

        Assert.Equal("60 ms, hit on frame 3 (Database > Effect strips)", catalog.StripPaceText);
    }

    [Fact]
    public void StripPaceText_UnlistedStrip_SaysUnlisted()
    {
        var catalog = new SkillCatalogViewModel(Catalog(), SkillFixtures.NoChoices);

        Assert.Equal("80 ms, hit on frame 1, unlisted (Database > Effect strips)", catalog.StripPaceText);
    }

    [Fact]
    public void StripPaceText_NoStrip_IsEmpty()
    {
        var catalog = new SkillCatalogViewModel(Catalog(), SkillFixtures.NoChoices);

        catalog.SelectedSkill!.EffectFile = " ";

        Assert.Equal("", catalog.StripPaceText);
    }

    [Fact]
    public void StripPaceText_StripTyped_IsRaised()
    {
        var catalog = new SkillCatalogViewModel(Catalog(), SkillFixtures.NoChoices);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.SelectedSkill!.EffectFile = "Spark.png";

        Assert.Contains(nameof(SkillCatalogViewModel.StripPaceText), notified);
    }

    [Fact]
    public void SelectedSkill_Chosen_RaisesItsLines()
    {
        var catalog = new SkillCatalogViewModel(Catalog(), SkillFixtures.NoChoices);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.AddSkillCommand.Execute(null);

        Assert.Contains(nameof(SkillCatalogViewModel.LearnedByText), notified);
        Assert.Contains(nameof(SkillCatalogViewModel.UsedByText), notified);
        Assert.Contains(nameof(SkillCatalogViewModel.StripPaceText), notified);
    }

    [Fact]
    public void LearnedByText_SavedSkill_NamesItsClass()
    {
        var mage = SkillFixtures.TestWorld.Classes["mage"] with { Name = "Mage", SkillIds = ["bolt"] };

        var catalog = new SkillCatalogViewModel(Catalog() with { Classes = ById(mage.Id, mage) }, SkillFixtures.NoChoices);

        Assert.Equal("Learned by: Mage", catalog.LearnedByText);
    }

    [Fact]
    public void LearnedByText_NewSkill_IsEmpty()
    {
        var catalog = new SkillCatalogViewModel(Catalog(), SkillFixtures.NoChoices);

        catalog.AddSkillCommand.Execute(null);

        Assert.Equal("", catalog.LearnedByText);
    }

    [Fact]
    public void UsedByText_SavedSkill_NamesTheEnemy()
    {
        var chief = EnemyNaming("bolt");

        var catalog = new SkillCatalogViewModel(Catalog() with { Enemies = ById(chief.Id, chief) }, SkillFixtures.NoChoices);

        Assert.Equal("Used by: Goblin Chief", catalog.UsedByText);
    }

    // The enemy names the id typed, so only the new skill's rule leaves the line empty.
    [Fact]
    public void UsedByText_NewSkillTypedAsAnEnemysSkill_IsEmpty()
    {
        var chief = EnemyNaming("spark");
        var catalog = new SkillCatalogViewModel(Catalog() with { Enemies = ById(chief.Id, chief) }, SkillFixtures.NoChoices);

        catalog.AddSkillCommand.Execute(null);
        catalog.SelectedSkill!.IdText = "spark";

        Assert.Equal("", catalog.UsedByText);
    }

    private static ContentCatalog Catalog() => SkillFixtures.CatalogWithoutUsers([SkillFixtures.Bolt], [SkillFixtures.Fire1]);

    private static EnemyDefinition EnemyNaming(string skillId) =>
        SkillFixtures.TestWorld.Enemies["goblin"] with
        {
            Name = "Goblin Chief",
            Tactics = [],
            Phases = [new EnemyPhase { BelowHpPercent = 50, Tactics = [new EnemyAction { SkillId = skillId, Weight = 1 }] }],
        };

    private static Dictionary<string, T> ById<T>(string id, T record) => new(StringComparer.Ordinal) { [id] = record };
}
