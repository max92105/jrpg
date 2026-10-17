using Jrpg.Core.Party;
using Jrpg.Data.Content;
using Jrpg.Data.Party;

namespace Jrpg.Data.Tests.Party;

public sealed class ClassCatalogValidatorTests
{
    [Fact]
    public void Validate_DuplicateId_ThrowsNamingTheId()
    {
        ClassDefinition[] classes = [new("warrior", "Warrior"), new("warrior", "Berserker")];

        var exception = Assert.Throws<ContentValidationException>(() => ClassCatalogValidator.Validate("classes.json", classes));

        Assert.Equal("classes.json: class id 'warrior' is used more than once.", exception.Message);
    }

    [Fact]
    public void Validate_EmptyName_ThrowsNamingTheClass()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ClassCatalogValidator.Validate("classes.json", [new ClassDefinition("warrior", "")]));

        Assert.Equal("classes.json: class 'warrior': name must not be empty.", exception.Message);
    }
}
