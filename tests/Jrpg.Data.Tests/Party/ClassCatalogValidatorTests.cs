using Jrpg.Core.Party;
using Jrpg.Data.Content;
using Jrpg.Data.Party;

namespace Jrpg.Data.Tests.Party;

public sealed class ClassCatalogValidatorTests
{
    [Fact]
    public void Validate_DuplicateId_ThrowsNamingTheId()
    {
        ClassDefinition[] classes = [new("warrior", "Warrior", "Fights."), new("warrior", "Berserker", "Rages.")];

        var exception = Assert.Throws<ContentValidationException>(() => ClassCatalogValidator.Validate("classes.json", classes));

        Assert.Equal("classes.json: class id 'warrior' is used more than once.", exception.Message);
    }

    [Fact]
    public void Validate_EmptyName_ThrowsNamingTheClass()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ClassCatalogValidator.Validate("classes.json", [new ClassDefinition("warrior", "", "Fights.")]));

        Assert.Equal("classes.json: class 'warrior': name must not be empty.", exception.Message);
    }

    [Fact]
    public void Validate_EmptyDescription_ThrowsNamingTheClass()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ClassCatalogValidator.Validate("classes.json", [new ClassDefinition("warrior", "Warrior", " ")]));

        Assert.Equal("classes.json: class 'warrior': description must not be empty.", exception.Message);
    }
}
