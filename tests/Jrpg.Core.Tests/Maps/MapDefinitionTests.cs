using Jrpg.Core.Maps;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Maps;

public sealed class MapDefinitionTests
{
    [Fact]
    public void Constructor_SettingsForAnotherMap_Throws()
    {
        var layout = new MapLayoutBuilder().WithLayer("1").Build();
        var settings = new MapSettings { Id = "forest", DisplayName = "Forest" };

        var exception = Assert.Throws<ArgumentException>(() => new MapDefinition(layout, settings));

        Assert.Equal("settings", exception.ParamName);
    }

    [Fact]
    public void Id_MatchingSettings_IsTheLayoutId()
    {
        var layout = new MapLayoutBuilder().WithLayer("1").Build();
        var settings = new MapSettings { Id = layout.Id, DisplayName = "Test Map" };

        var map = new MapDefinition(layout, settings);

        Assert.Equal("test_map", map.Id);
    }
}
