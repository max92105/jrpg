using Jrpg.Core.Battle;
using Jrpg.Core.Cinematics;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Trade;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The catalogs the Database windows edit (ADR 0045): the shops, the enemies, the encounters, the drop tables, the
/// cinematics, the effect strips, the animated backgrounds, the items and the affix pool, the whole session of each
/// window recorded as one undo step.
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
    /// Replaces the animated battle backgrounds' records as one undo step (decision 901); nothing when they are the
    /// same by content. No undo refusal: nothing refers to a record, a background is named by its file.
    /// </summary>
    public void SetAnimatedBackgrounds(IReadOnlyList<AnimatedBackgroundDefinition> backgrounds)
    {
        var before = Current.AnimatedBackgrounds;
        var after = backgrounds.ToDictionary(background => background.File, StringComparer.Ordinal);
        if (HasSameRecords(before, after, EqualityComparer<AnimatedBackgroundDefinition>.Default.Equals))
        {
            return;
        }

        Do(new CatalogEdit(
            catalog => catalog with { AnimatedBackgrounds = after },
            catalog => catalog with { AnimatedBackgrounds = before }));
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
    /// phases included. The encounters naming an enemy whose id was retyped name its new one in the same step, so an
    /// undo brings both back. No undo refusal: only encounters name an enemy, and catalog steps undo in order, so an
    /// encounter's pick of an added enemy is undone before the enemy.
    /// </summary>
    /// <param name="enemies">Every enemy as edited.</param>
    /// <param name="renamedIds">Each retyped id of the draft's enemies with its new one; empty when none was.</param>
    public void SetEnemies(IReadOnlyList<EnemyDefinition> enemies, IReadOnlyDictionary<string, string> renamedIds)
    {
        var before = Current.Enemies;
        var after = enemies.ToDictionary(enemy => enemy.Id, StringComparer.Ordinal);
        if (HasSameRecords(before, after, IsSameEnemy))
        {
            return;
        }

        var encountersBefore = Current.Encounters;
        var encountersAfter = WithEnemiesRenamed(encountersBefore, renamedIds);
        Do(new CatalogEdit(
            catalog => catalog with { Enemies = after, Encounters = encountersAfter },
            catalog => catalog with { Enemies = before, Encounters = encountersBefore }));
    }

    // An encounter naming no renamed enemy stays the same record, so the encounters' own step still compares it equal.
    private static Dictionary<string, EncounterDefinition> WithEnemiesRenamed(
        IReadOnlyDictionary<string, EncounterDefinition> encounters, IReadOnlyDictionary<string, string> renamedIds) =>
        encounters.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.EnemyIds.Any(renamedIds.ContainsKey)
                ? pair.Value with { EnemyIds = [.. pair.Value.EnemyIds.Select(id => renamedIds.GetValueOrDefault(id, id))] }
                : pair.Value,
            StringComparer.Ordinal);

    /// <summary>
    /// Replaces the encounters as one undo step (decision 905); nothing when they are the same by content, enemies in
    /// slot order included.
    /// </summary>
    public void SetEncounters(IReadOnlyList<EncounterDefinition> encounters)
    {
        var before = Current.Encounters;
        var after = encounters.ToDictionary(encounter => encounter.Id, StringComparer.Ordinal);
        if (HasSameRecords(before, after, IsSameEncounter))
        {
            return;
        }

        Do(new CatalogEdit(catalog => catalog with { Encounters = after }, catalog => catalog with { Encounters = before }));
    }

    /// <summary>
    /// Replaces the items as one undo step (decision 877); nothing when they are the same by content, a unique's fixed
    /// affixes in order included. An undo removing one a map's event gives is refused (decision 883); shops and drop
    /// tables naming an added item came in later steps, undone first, so they need no refusal.
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

    /// <summary>
    /// Replaces the affix pool as one undo step (decision 884), every unique's fixed affixes taking their stats from
    /// the new pool, so a stat changed on an affix reaches the uniques naming it; an undo restores both. Nothing when
    /// the pool is the same by content, kinds in order included. No undo refusal: only items name an affix, the Affixes
    /// page removes only one added this session, and catalog steps undo in order.
    /// </summary>
    public void SetAffixes(IReadOnlyList<AffixDefinition> affixes)
    {
        var (affixesBefore, itemsBefore) = (Current.Affixes, Current.Items);
        var affixesAfter = affixes.ToDictionary(affix => affix.Id, StringComparer.Ordinal);
        if (HasSameRecords(affixesBefore, affixesAfter, EqualityComparer<AffixDefinition>.Default.Equals))
        {
            return;
        }

        var itemsAfter = itemsBefore.Values.ToDictionary(
            item => item.Id, item => WithFixedStatsFrom(affixesAfter, item), StringComparer.Ordinal);
        Do(new CatalogEdit(
            catalog => catalog with { Affixes = affixesAfter, Items = itemsAfter },
            catalog => catalog with { Affixes = affixesBefore, Items = itemsBefore }));
    }

    // A fixed affix holds its affix's stat, resolved from the pool as items.json is read (decision 865); a unique's
    // value stays whatever the new range (decision 866).
    private static ItemDefinition WithFixedStatsFrom(Dictionary<string, AffixDefinition> pool, ItemDefinition item) =>
        item.IsUnique
            ? item with { FixedAffixes = [.. item.FixedAffixes.Select(line => line with { Stat = pool[line.AffixId].Stat })] }
            : item;

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

    // As an enemy's: the with-copy compares every field but the enemy list, the list by content and order (slot order
    // is screen order).
    private static bool IsSameEncounter(EncounterDefinition before, EncounterDefinition after) =>
        before with { EnemyIds = after.EnemyIds } == after && before.EnemyIds.SequenceEqual(after.EnemyIds);

    /// <summary>The shops the next undo would remove, so it can be refused while a map's event opens one.</summary>
    public IReadOnlyList<string> ShopsRemovedByUndo() => KeysRemovedByUndo(catalog => catalog.Shops);

    /// <summary>
    /// The cinematics the next undo would remove, so it can be refused while a map's event plays one.
    /// </summary>
    public IReadOnlyList<string> CinematicsRemovedByUndo() => KeysRemovedByUndo(catalog => catalog.Cinematics);

    /// <summary>The items the next undo would remove, so it can be refused while a map's event gives one.</summary>
    public IReadOnlyList<string> ItemsRemovedByUndo() => KeysRemovedByUndo(catalog => catalog.Items);

    /// <summary>
    /// The encounters the next undo would remove, so it can be refused while a map's random battles roll one.
    /// </summary>
    public IReadOnlyList<string> EncountersRemovedByUndo() => KeysRemovedByUndo(catalog => catalog.Encounters);
}
