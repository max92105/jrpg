using Jrpg.Core.Battle;
using Jrpg.Data.Battle;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Battle;

public sealed class EncounterCatalogValidatorTests
{
    private static readonly HashSet<string> _enemyIds = ["goblin"];

    private static readonly EncounterDefinition _goblinTrio = new()
    {
        Id = "goblin_trio",
        EnemyIds = ["goblin", "goblin", "goblin"],
        BackgroundFile = "Forest.png",
    };

    [Fact]
    public void Validate_ValidEncounter_DoesNotThrow()
    {
        var exception = Record.Exception(() => EncounterCatalogValidator.Validate("encounters.json", [_goblinTrio], _enemyIds));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_UnknownEnemyId_ThrowsNamingTheEncounter()
    {
        var encounter = _goblinTrio with { EnemyIds = ["goblin", "dragon"] };

        var exception = Assert.Throws<ContentValidationException>(
            () => EncounterCatalogValidator.Validate("encounters.json", [encounter], _enemyIds));

        Assert.Equal("encounters.json: encounter 'goblin_trio': enemyIds has unknown enemy id 'dragon'.", exception.Message);
    }

    [Fact]
    public void Validate_NoEnemies_ThrowsNamingTheEncounter()
    {
        var encounter = _goblinTrio with { EnemyIds = [] };

        var exception = Assert.Throws<ContentValidationException>(
            () => EncounterCatalogValidator.Validate("encounters.json", [encounter], _enemyIds));

        Assert.Equal("encounters.json: encounter 'goblin_trio': enemyIds must list at least one enemy.", exception.Message);
    }
}
