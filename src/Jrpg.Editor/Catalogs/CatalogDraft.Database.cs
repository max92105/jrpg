using Jrpg.Core.Battle;
using Jrpg.Core.Trade;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The catalogs the Database windows edit (ADR 0045): the shops and the enemies, each window's session one step.
/// </summary>
internal sealed partial class CatalogDraft
{
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
    public IReadOnlyList<string> ShopsRemovedByUndo() =>
        _history.NextUndo is { } edit
            ? [.. Current.Shops.Keys.Except(edit.Revert(Current).Shops.Keys).Order(StringComparer.Ordinal)]
            : [];
}
