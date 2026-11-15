using System.Windows.Input;

namespace Jrpg.Editor.Events;

/// <summary>
/// One entry of an event dialog's drop-down (decision 730): its words, and the command and parameter it runs. An entry
/// without a command is a group's title: shown greyed, never chosen.
/// </summary>
/// <param name="Label">The entry's words, shown as plain text.</param>
/// <param name="Command">What choosing it runs; null for a title.</param>
/// <param name="Parameter">What the command is given, such as the step kind to add.</param>
internal sealed record DropDownChoice(string Label, ICommand? Command, object? Parameter)
{
    public bool IsTitle => Command is null;

    /// <summary>A group's title, in capitals so it doesn't read as a greyed entry (decision 729).</summary>
    public static DropDownChoice Title(string label) => new(label, null, null);
}
