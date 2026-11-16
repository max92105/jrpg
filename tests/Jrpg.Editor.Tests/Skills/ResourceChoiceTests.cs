using System.Collections.Frozen;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Editor.Skills;

namespace Jrpg.Editor.Tests.Skills;

// What the cost boxes offer (decision 979): every class's resources once, labelled with their classes, then the ones a
// skill names that no class has; the classes are built here so no repository list is pinned, in a frozen dictionary as
// the loader keeps them.
public sealed class ResourceChoiceTests
{
    private static readonly ClassDefinition _mage = SkillFixtures.TestWorld.Classes["mage"];
    private static readonly ResourceDefinition _mana = new("mana", "Mana", 10);

    // Listed against id order, and the warlock names it otherwise: the label takes the first class's name by id.
    [Fact]
    public void AllIn_ResourceOfTwoClasses_ListedOnceNamingBoth()
    {
        var mage = _mage with { Resources = [_mana] };
        var warlock = _mage with { Id = "warlock", Resources = [_mana with { Name = "Arcana" }] };

        var choices = ResourceChoice.AllIn(ContentOf([warlock, mage], []));

        Assert.Equal([new ResourceChoice("mana", "Mana (mage, warlock)")], choices);
    }

    // Listed against id order, and a sort of the resources by id would put clarity first.
    [Fact]
    public void AllIn_TwoClasses_ClassesByIdEachInItsOwnOrder()
    {
        var warlock = _mage with { Id = "warlock", Resources = [new ResourceDefinition("instability", "Instability", 10)] };
        var mage = _mage with { Resources = [_mana, new ResourceDefinition("clarity", "Clarity", 3)] };

        var choices = ResourceChoice.AllIn(ContentOf([warlock, mage], []));

        Assert.Equal(["mana", "clarity", "instability"], choices.Select(choice => choice.Id));
    }

    // A skill no class learns may name any resource; its box must still show it.
    [Fact]
    public void AllIn_ResourceOfNoClass_ListedLastByItsId()
    {
        var mage = _mage with { Resources = [_mana] };
        var skill = SkillFixtures.Bolt with
        {
            ResourceChanges = [new ResourceChange("zeal", -1), new ResourceChange("mana", -1)],
            Requirements = [new ResourceRequirement("focus", 1)],
        };

        var choices = ResourceChoice.AllIn(ContentOf([mage], [skill]));

        Assert.Equal(
            [new("mana", "Mana (mage)"), new("focus", "focus (no class)"), new ResourceChoice("zeal", "zeal (no class)")],
            choices);
    }

    private static ContentCatalog ContentOf(ClassDefinition[] classes, SkillDefinition[] skills) =>
        SkillFixtures.TestWorld with
        {
            Classes = classes.ToFrozenDictionary(characterClass => characterClass.Id, StringComparer.Ordinal),
            Skills = skills.ToDictionary(skill => skill.Id, StringComparer.Ordinal),
        };
}
