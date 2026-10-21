namespace Jrpg.Editor.Shell;

/// <summary>One row of the map list.</summary>
/// <param name="Id">The map's id, which opens it.</param>
/// <param name="Text">What the list shows: the display name, then the id.</param>
internal sealed record MapEntry(string Id, string Text);
