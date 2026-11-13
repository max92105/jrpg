using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Loot;
using Jrpg.Core.Sprites;
using Jrpg.Core.Trade;
using Jrpg.Data.Battle;
using Jrpg.Data.Loot;
using Jrpg.Data.Sprites;
using Jrpg.Data.Trade;

namespace Jrpg.Data.Content;

/// <summary>
/// What the loader would refuse in a catalog the editor is editing, as the loader's own message, so a dialog's problem
/// line says exactly what the save and the game's start would say and no editor rule copies a content rule. The
/// writer's checks run the same methods, so the two can't drift.
/// </summary>
public static class CatalogProblems
{
    /// <summary>
    /// The first problem in the field sprite records (ADR 0043): a record's own rule, or an object or the new game
    /// naming a sprite none of them describes; null when the loader would take them.
    /// </summary>
    /// <param name="sheets">The records as edited.</param>
    /// <param name="content">The objects and the new game that name sprites.</param>
    public static string? OfFieldSprites(IReadOnlyList<FieldSpriteSheet> sheets, ContentCatalog content) =>
        FirstProblem(() => CheckFieldSprites(sheets, content));

    /// <summary>
    /// The first problem in the shops (ADR 0031, ADR 0045): a shop's own rule, or an item it sells that the content
    /// doesn't price; null when the loader would take them.
    /// </summary>
    /// <param name="shops">The shops as edited.</param>
    /// <param name="content">The items the shops sell.</param>
    public static string? OfShops(IReadOnlyList<ShopDefinition> shops, ContentCatalog content) =>
        FirstProblem(() => CheckShops(shops, content));

    /// <summary>
    /// The first problem in the enemies (ADR 0034, ADR 0045): an enemy's own numbers, its drop table, or a tactic
    /// naming a skill the content lacks or enemies can't use; null when the loader would take them.
    /// </summary>
    /// <param name="enemies">The enemies as edited.</param>
    /// <param name="content">The drop tables and skills an enemy may name.</param>
    public static string? OfEnemies(IReadOnlyList<EnemyDefinition> enemies, ContentCatalog content) =>
        FirstProblem(() => CheckEnemies(enemies, content));

    /// <summary>
    /// The first problem in the drop tables (decision 343, ADR 0045): a table's own rule, or an entry naming an item
    /// the content lacks or a quantity its stack can't hold; null when the loader would take them.
    /// </summary>
    /// <param name="tables">The drop tables as edited.</param>
    /// <param name="content">The items the entries name.</param>
    public static string? OfDropTables(IReadOnlyList<DropTable> tables, ContentCatalog content) =>
        FirstProblem(() => CheckDropTables(tables, content));

    /// <summary>
    /// The checks the loader makes on <c>field-sprites.json</c>, for the writer and the problem line alike.
    /// </summary>
    /// <exception cref="ContentValidationException">The first problem, naming the file.</exception>
    internal static void CheckFieldSprites(IReadOnlyList<FieldSpriteSheet> sheets, ContentCatalog content)
    {
        FieldSpriteCatalogValidator.Validate(ContentCatalogLoader.FieldSpritesFile, sheets);
        var byFile = sheets.ToDictionary(sheet => sheet.File, StringComparer.Ordinal);
        FieldSpriteReferenceCheck.Check(ContentCatalogLoader.FieldSpritesFile, byFile, content.Objects.Values, content.NewGame);
    }

    /// <summary>The checks the loader makes on <c>shops.json</c>, for the writer and the problem line alike.</summary>
    /// <exception cref="ContentValidationException">The first problem, naming the file.</exception>
    internal static void CheckShops(IReadOnlyList<ShopDefinition> shops, ContentCatalog content) =>
        ShopCatalogValidator.Validate(ContentCatalogLoader.ShopsFile, shops, content.Items);

    /// <summary>
    /// The checks the loader makes on <c>enemies.json</c>, for the writer and the problem line alike.
    /// </summary>
    /// <exception cref="ContentValidationException">The first problem, naming the file.</exception>
    internal static void CheckEnemies(IReadOnlyList<EnemyDefinition> enemies, ContentCatalog content) =>
        EnemyCatalogValidator.Validate(
            ContentCatalogLoader.EnemiesFile, enemies, new EnemyReferences(content.DropTables.Keys.ToHashSet(), content.Skills));

    /// <summary>
    /// The checks the loader makes on <c>drop-tables.json</c>, for the writer and the problem line alike.
    /// </summary>
    /// <exception cref="ContentValidationException">The first problem, naming the file.</exception>
    internal static void CheckDropTables(IReadOnlyList<DropTable> tables, ContentCatalog content) =>
        DropTableCatalogValidator.Validate(ContentCatalogLoader.DropTablesFile, tables, content.Items);

    // The loader's refusal as a line, or null when it would take the catalog.
    private static string? FirstProblem(Action check)
    {
        try
        {
            check();
            return null;
        }
        catch (ContentValidationException exception)
        {
            return exception.Message;
        }
    }
}
