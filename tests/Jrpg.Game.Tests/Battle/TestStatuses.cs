using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;

namespace Jrpg.Game.Tests.Battle;

/// <summary>
/// The statuses the battle screen's tests put in events, in one place, so the status model can change under them
/// (decision 811): records built here, each of one part and its own family, named as the repository's are.
/// </summary>
internal static class TestStatuses
{
    public static StatusDefinition Stun { get; } =
        Record("stun", "Stun", new StatusPart(StatusPartKind.SkipTurn)) with { IsHarmful = true };

    public static StatusDefinition Poison { get; } =
        Record("poison_1", "Poison", new StatusPart(StatusPartKind.DamageEachTurn, 8)) with { IsHarmful = true };

    public static StatusDefinition Burning { get; } =
        Record("burning_1", "Burning", new StatusPart(StatusPartKind.DamageEachTurn, 5, DamageType.Fire))
        with
        { IsHarmful = true, ActsAt = StatusTiming.StartOfTurn };

    public static StatusDefinition Regen { get; } = Record("regen_1", "Regen", new StatusPart(StatusPartKind.HealEachTurn, 6));

    public static StatusDefinition Record(string id, string name, StatusPart part) =>
        new() { Id = id, Name = name, Description = "", Parts = [part] };
}
