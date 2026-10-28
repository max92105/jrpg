using Jrpg.Core.Stats;

namespace Jrpg.Core.Items;

/// <summary>
/// One piece of gear as owned or worn (ADR 0029): a base item that is equipment, a rarity and the affixes it rolled. A
/// value: two pieces are equal when their base, rarity and affixes (in roll order) are, so two plain bronze swords are
/// interchangeable.
/// </summary>
public sealed class GearPiece : IEquatable<GearPiece>
{
    private readonly EquipmentStats _equipment;

    private GearPiece(ItemDefinition item, EquipmentStats equipment, Rarity rarity, IReadOnlyList<RolledAffix> affixes)
    {
        Base = item;
        _equipment = equipment;
        Rarity = rarity;
        Affixes = affixes;
        Bonuses = affixes.Aggregate(equipment.Bonuses, (total, affix) => total.Plus(BonusOf(affix)));
    }

    /// <summary>The catalog item the piece is one of: its name, icon, kind and base numbers.</summary>
    public ItemDefinition Base { get; }

    public Rarity Rarity { get; }

    /// <summary>In roll order.</summary>
    public IReadOnlyList<RolledAffix> Affixes { get; }

    public EquipmentKind Kind => _equipment.Kind;

    /// <summary>A weapon's raw damage (decision 330); 0 for anything else.</summary>
    public int Damage => _equipment.Damage;

    /// <summary>A weapon's hits' type (ADR 0027); null for anything else.</summary>
    public DamageType? DamageType => _equipment.DamageType;

    /// <summary>What wearing the piece adds: the base item's bonuses plus every affix's.</summary>
    public StatBonuses Bonuses { get; }

    /// <summary>
    /// A Common piece without affixes: a starting item, a map gift, or any fixed-stat item (decision 344).
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="item"/> isn't equipment.</exception>
    public static GearPiece Plain(ItemDefinition item) => Rolled(item, Rarity.Common, []);

    /// <param name="item">The base item.</param>
    /// <param name="rarity">What the roll gave.</param>
    /// <param name="affixes">In roll order; copied.</param>
    /// <exception cref="ArgumentException"><paramref name="item"/> isn't equipment.</exception>
    public static GearPiece Rolled(ItemDefinition item, Rarity rarity, IReadOnlyList<RolledAffix> affixes) =>
        item.Equipment is { } equipment
            ? new GearPiece(item, equipment, rarity, [.. affixes])
            : throw new ArgumentException($"Item '{item.Id}' is not equipment.", nameof(item));

    // The base compares by id: a catalog holds one item per id.
    public bool Equals(GearPiece? other) =>
        other is not null && Base.Id == other.Base.Id && Rarity == other.Rarity && Affixes.SequenceEqual(other.Affixes);

    public override bool Equals(object? obj) => Equals(obj as GearPiece);

    public static bool operator ==(GearPiece? left, GearPiece? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(GearPiece? left, GearPiece? right) => !(left == right);

    public override int GetHashCode() =>
        Affixes.Aggregate(HashCode.Combine(Base.Id, Rarity), (hash, affix) => HashCode.Combine(hash, affix));

    private static StatBonuses BonusOf(RolledAffix affix) => new(new Dictionary<StatKind, int> { [affix.Stat] = affix.Value });
}
