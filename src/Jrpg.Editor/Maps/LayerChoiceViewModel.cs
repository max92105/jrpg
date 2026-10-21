using System.Globalization;
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

    /// <summary>
    /// Chooses the layer typed in the Add layer box: a preset's name or any whole number. The map gets the layer when
    /// something is painted on it.
    /// </summary>
    /// <returns>False, keeping the choice, when the text is neither.</returns>
    public bool TryAddLayer(string text)
    {
        var trimmed = text.Trim();
        if (Presets.FirstOrDefault(preset => string.Equals(preset.Name, trimmed, StringComparison.OrdinalIgnoreCase)) is { } preset)
        {
            Z = preset.Z;
            return true;
        }

        if (!int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var z))
        {
            return false;
        }

        Z = z;
        return true;
    }
}
