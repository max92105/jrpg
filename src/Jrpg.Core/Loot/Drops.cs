using Jrpg.Core.Items;

namespace Jrpg.Core.Loot;

/// <summary>What a won battle dropped: stacks in first-drop order, then gear pieces in drop order.</summary>
public sealed record Drops(IReadOnlyList<ItemDrop> Stacks, IReadOnlyList<GearPiece> Gear)
{
    public static Drops None { get; } = new([], []);

    public bool IsEmpty => Stacks.Count == 0 && Gear.Count == 0;
}
