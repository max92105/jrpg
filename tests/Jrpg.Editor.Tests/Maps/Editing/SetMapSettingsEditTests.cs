using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Maps.Editing;

public sealed class SetMapSettingsEditTests
{
    private static readonly MapDefinition _meadow = new(
        new MapLayout { Id = "meadow", Width = 1, Height = 1, Layers = [], Objects = [] },
        new MapSettings { Id = "meadow", DisplayName = "Meadow" });

    [Fact]
    public void Revert_RestoresTheSettings()
    {
        var draft = MapDraft.From(_meadow);
        var edit = new SetMapSettingsEdit(_meadow.Settings, _meadow.Settings with { DisplayName = "Sunny meadow" });
        edit.Apply(draft);
        Assert.Equal("Sunny meadow", draft.ToDefinition().Settings.DisplayName);

        edit.Revert(draft);

        Assert.Equal(_meadow.Settings, draft.ToDefinition().Settings);
    }
}
