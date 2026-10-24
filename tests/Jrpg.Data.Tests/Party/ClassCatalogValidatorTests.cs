using Jrpg.Core.Party;
using Jrpg.Data.Content;
using Jrpg.Data.Party;

namespace Jrpg.Data.Tests.Party;

public sealed class ClassCatalogValidatorTests
{
    [Fact]
    public void Validate_DuplicateId_ThrowsNamingTheId()
    {
        ClassDefinition[] classes = [Class("warrior") with { Name = "Warrior" }, Class("warrior") with { Name = "Berserker" }];

        var exception = Assert.Throws<ContentValidationException>(() => ClassCatalogValidator.Validate("classes.json", classes));

        Assert.Equal("classes.json: class id 'warrior' is used more than once.", exception.Message);
    }

    [Fact]
    public void Validate_EmptyName_ThrowsNamingTheClass()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ClassCatalogValidator.Validate("classes.json", [Class("warrior") with { Name = "" }]));

        Assert.Equal("classes.json: class 'warrior': name must not be empty.", exception.Message);
    }

    [Fact]
    public void Validate_EmptyDescription_ThrowsNamingTheClass()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ClassCatalogValidator.Validate("classes.json", [Class("warrior") with { Description = " " }]));

        Assert.Equal("classes.json: class 'warrior': description must not be empty.", exception.Message);
    }

    public static TheoryData<ClassDefinition, string> InvalidStats => new()
    {
        { Class("warrior") with { Base = Stats with { MaxHp = 0 } }, "base.maxHp must be at least 1 (was 0)." },
        { Class("warrior") with { Base = Stats with { MaxMp = -1 } }, "base.maxMp must be at least 0 (was -1)." },
        { Class("warrior") with { Base = Stats with { Attack = -2 } }, "base.attack must be at least 0 (was -2)." },
        { Class("warrior") with { Growth = Stats with { Defense = -1 } }, "growth.defense must be at least 0 (was -1)." },
        { Class("warrior") with { Growth = Stats with { MaxHp = -5 } }, "growth.maxHp must be at least 0 (was -5)." },
    };

    private static ClassStats Stats { get; } = new(MaxHp: 200, MaxMp: 20, Attack: 20, Defense: 5);

    [Theory]
    [MemberData(nameof(InvalidStats))]
    public void Validate_InvalidStat_ThrowsNamingTheClassAndField(ClassDefinition classDefinition, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(() => ClassCatalogValidator.Validate("classes.json", [classDefinition]));

        Assert.Equal($"classes.json: class 'warrior': {problem}", exception.Message);
    }

    [Fact]
    public void Validate_ZeroGrowth_DoesNotThrow()
    {
        var neverImproving = Class("warrior") with { Growth = new ClassStats(MaxHp: 0, MaxMp: 0, Attack: 0, Defense: 0) };

        var exception = Record.Exception(() => ClassCatalogValidator.Validate("classes.json", [neverImproving]));

        Assert.Null(exception);
    }

    private static ClassDefinition Class(string id) => new(id, "Warrior", "Fights.", Stats, Stats);
}
