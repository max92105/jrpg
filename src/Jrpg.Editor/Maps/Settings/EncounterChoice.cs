using CommunityToolkit.Mvvm.ComponentModel;

namespace Jrpg.Editor.Maps.Settings;

/// <summary>One encounter in the settings' list, ticked when random battles on the map can pick it.</summary>
internal sealed partial class EncounterChoice(string encounterId) : ObservableObject
{
    public string EncounterId { get; } = encounterId;

    [ObservableProperty]
    public partial bool IsChosen { get; set; }
}
