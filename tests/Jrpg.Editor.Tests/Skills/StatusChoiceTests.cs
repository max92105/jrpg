using System.Collections.Frozen;
using Jrpg.Core.Statuses;
using Jrpg.Editor.Skills;

namespace Jrpg.Editor.Tests.Skills;

// What the grant boxes offer (decision 981): the draft's statuses by id, each with its name; set up in a frozen
// dictionary as the loader keeps them.
public sealed class StatusChoiceTests
{
    // Listed against id order: a box in the dictionary's order would put stun first.
    [Fact]
    public void AllIn_TwoStatuses_ByIdWithTheirNames()
    {
        var stun = SkillFixtures.Poison1 with { Id = "stun", Name = "Stun" };
        StatusDefinition[] statuses = [stun, SkillFixtures.Poison1];
        var content = SkillFixtures.TestWorld with
        {
            Statuses = statuses.ToFrozenDictionary(status => status.Id, StringComparer.Ordinal),
        };

        var choices = StatusChoice.AllIn(content);

        Assert.Equal([new StatusChoice("poison_1", "poison_1: Poison"), new StatusChoice("stun", "stun: Stun")], choices);
    }
}
