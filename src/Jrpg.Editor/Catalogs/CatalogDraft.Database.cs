using Jrpg.Core.Battle;
using Jrpg.Core.Cinematics;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Trade;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The catalogs the Database windows edit (ADR 0045): the shops, the enemies, the drop tables, the cinematics, the
/// effect strips and the items, the whole session of each window recorded as one undo step.
/// </summary>
internal sealed partial class CatalogDraft
{
    /// <summary>
    /// Replaces the drop tables as one undo step (decision 591); nothing when they are the same by content, entries in
    /// order included. No undo refusal (decision 599): only enemies name a table, and catalog steps undo in order, so
    /// an enemy's pick of an added table is undone before the table.
    /// </summary>
    public void SetDropTables(IReadOnlyList<DropTable> tables)
    {
        var before = Current.DropTables;
        var after = tables.ToDictionary(table => table.Id, StringComparer.Ordinal);
        if (HasSameRecords(before, after, EqualityComparer<DropTable>.Default.Equals))
        {
            return;
        }

        Do(new CatalogEdit(catalog => catalog with { DropTables = after }, catalog => catalog with { DropTables = before }));
    }

    /// <summary>
    /// Replaces the effect strips' records as one undo step (decision 659); nothing when they are the same by content.
    /// No undo refusal: nothing refers to a record, a strip is named by its file.
    /// </summary>
    public void SetEffectStrips(IReadOnlyList<EffectStripDefinition> strips)
    {
        var before = Current.EffectStrips;
        var after = strips.ToDictionary(strip => strip.File, StringComparer.Ordinal);
        if (HasSameRecords(before, after, EqualityComparer<EffectStripDefinition>.Default.Equals))
        {
            return;
        }

        Do(new CatalogEdit(catalog => catalog with { EffectStrips = after }, catalog => catalog with { EffectStrips = before }));
    }

    /// <summary>
    /// Replaces the cinematics as one undo step (ADR 0045); nothing when they are the same by content, stills in order
    /// included. An undo removing one a map's event plays is refused, as for shops.
    /// </summary>
    public void SetCinematics(IReadOnlyList<CinematicDefinition> cinematics)
    {
        var before = Current.Cinematics;
        var after = cinematics.ToDictionary(cinematic => cinematic.Id, StringComparer.Ordinal);
        if (HasSameRecords(before, after, EqualityComparer<CinematicDefinition>.Default.Equals))
        {
            return;
        }

        Do(new CatalogEdit(catalog => catalog with { Cinematics = after }, catalog => catalog with { Cinematics = before }));
    }

    /// <summary>
    /// Replaces the shops as one undo step (decision 591); nothing when they are the same by content, items included.
    /// </summary>
    public void SetShops(IReadOnlyList<ShopDefinition> shops)
    {
        var before = Current.Shops;
        var after = shops.ToDictionary(shop => shop.Id, StringComparer.Ordinal);
        if (HasSameRecords(before, after, EqualityComparer<ShopDefinition>.Default.Equals))
        {
            return;
        }

        Do(new CatalogEdit(catalog => catalog with { Shops = after }, catalog => catalog with { Shops = before }));
    }

    /// <summary>
    /// Replaces the enemies as one undo step (decision 591); nothing when they are the same by content, tactics and
    /// phases included. No undo refusal: only encounters name an enemy, and the editor doesn't edit them.
    /// </summary>
    public void SetEnemies(IReadOnlyList<EnemyDefinition> enemies)
    {
        var before = Current.Enemies;
        var after = enemies.ToDictionary(enemy => enemy.Id, StringComparer.Ordinal);
        if (HasSameRecords(before, after, IsSameEnemy))
        {
            return;
        }

        Do(new CatalogEdit(catalog => catalog with { Enemies = after }, catalog => catalog with { Enemies = before }));
    }

    /// <summary>
    /// Replaces the items as one undo step (decision 877); nothing when they are the same by content, a unique's fixed
    /// affixes in order included. No undo refusal yet: shops and drop tables naming an added item came in later steps,
    /// undone first, and the refusal for one a map's event gives is decision 883's, still to come.
    /// </summary>
    public void SetItems(IReadOnlyList<ItemDefinition> items)
    {
        var before = Current.Items;
        var after = items.ToDictionary(item => item.Id, StringComparer.Ordinal);
        if (HasSameRecords(before, after, EqualityComparer<ItemDefinition>.Default.Equals))
        {
            return;
        }

        Do(new CatalogEdit(catalog => catalog with { Items = after }, catalog => catalog with { Items = before }));
    }

    // A window's session changed nothing when it hands back the same ids and each record the same by content, so an OK
    // with no edit records no step (and keeps the redo).
    private static bool HasSameRecords<T>(
        IReadOnlyDictionary<string, T> before,
        IReadOnlyDictionary<string, T> after,
        Func<T, T, bool> isSame) =>
        before.Count == after.Count
        && after.All(pair => before.TryGetValue(pair.Key, out var record) && isSame(record, pair.Value));

    // Records compare lists by reference: the with-copy compares every field but the lists, the lists by content.
    private static bool IsSameEnemy(EnemyDefinition before, EnemyDefinition after) =>
        before with { Tactics = after.Tactics, Phases = after.Phases } == after
        && before.Tactics.SequenceEqual(after.Tactics)
        && before.Phases.Count == after.Phases.Count
        && before.Phases.Zip(after.Phases).All(pair =>
            pair.First.BelowHpPercent == pair.Second.BelowHpPercent && pair.First.Tactics.SequenceEqual(pair.Second.Tactics));

    /// <summary>The shops the next undo would remove, so it can be refused while a map's event opens one.</summary>
    public IReadOnlyList<string> ShopsRemovedByUndo() => KeysRemovedByUndo(catalog => catalog.Shops);

    /// <summary>
    /// The cinematics the next undo would remove, so it can be refused while a map's event plays one.
    /// </summary>
    public IReadOnlyList<string> CinematicsRemovedByUndo() => KeysRemovedByUndo(catalog => catalog.Cinematics);
}
