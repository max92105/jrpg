namespace Jrpg.Core.Maps;

/// <summary>One map as the game uses it: its layout with its settings (ADR 0011).</summary>
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
