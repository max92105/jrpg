namespace Jrpg.Editor.Events;

/// <summary>The kinds of event step the dialog edits, one per Core step type.</summary>
internal enum EventStepKind
{
    Dialogue,
    GiveItem,
    SwapObject,
    Warp,
    Still,
    Recruit,
}
