using CommunityToolkit.Mvvm.ComponentModel;

namespace Jrpg.Editor.Maps;

/// <summary>The layer edits go to: tiles are painted on it and objects placed on it, and the others can be dimmed.</summary>
internal sealed partial class LayerChoiceViewModel : ObservableObject
{
    public IReadOnlyList<LayerPreset> Presets { get; } = LayerPresets.All;

    /// <summary>Picked from <see cref="Presets"/> or typed in; the ground by default, as in the legacy editor.</summary>
    [ObservableProperty]
    public partial int Z { get; set; } = LayerPresets.GroundZ;

    /// <summary>Whether everything off <see cref="Z"/> is drawn faint.</summary>
    [ObservableProperty]
    public partial bool AreOtherLayersDimmed { get; set; }
}
