namespace Jrpg.Core.Maps;

/// <summary>One map as the game uses it: the generated layout with its hand-written settings.</summary>
public sealed record MapDefinition
{
    public MapDefinition(MapLayout layout, MapSettings settings)
    {
        if (settings.Id != layout.Id)
        {
            throw new ArgumentException($"Settings '{settings.Id}' don't belong to map '{layout.Id}'.", nameof(settings));
        }

        Layout = layout;
        Settings = settings;
    }

    public MapLayout Layout { get; }

    public MapSettings Settings { get; }

    public string Id => Layout.Id;
}
