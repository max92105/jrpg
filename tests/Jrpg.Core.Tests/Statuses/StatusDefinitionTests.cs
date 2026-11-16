using Jrpg.Core.Statuses;

namespace Jrpg.Core.Tests.Statuses;

// A status record compares its parts in order (decision 795) and is kept by its family, else its id (decision 800).
public sealed class StatusDefinitionTests
{
    [Fact]
    public void Equals_SamePartsInOrder_IsTrue()
    {
        Assert.Equal(Burning(Damage(5), Exposed(20)), Burning(Damage(5), Exposed(20)));
    }

    [Fact]
    public void Equals_PartsInAnotherOrder_IsFalse()
    {
        Assert.NotEqual(Burning(Damage(5), Exposed(20)), Burning(Exposed(20), Damage(5)));
    }

    [Fact]
    public void FamilyKey_WithoutFamily_IsTheId()
    {
        Assert.Equal("burning_1", Burning(Damage(5)).FamilyKey);
    }

    [Fact]
    public void FamilyKey_WithFamily_IsTheFamily()
    {
        Assert.Equal("burning", (Burning(Damage(5)) with { Family = "burning" }).FamilyKey);
    }

    private static StatusDefinition Burning(params StatusPart[] parts) =>
        new() { Id = "burning_1", Name = "Burning", Description = "", IsHarmful = true, Parts = [.. parts] };

    private static StatusPart Damage(int amount) => new(StatusPartKind.DamageEachTurn, amount);

    private static StatusPart Exposed(int amount) => new(StatusPartKind.DamageTakenChange, amount);
}
