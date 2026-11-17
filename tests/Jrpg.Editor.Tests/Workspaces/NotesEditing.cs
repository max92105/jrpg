using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Tests.Workspaces;

/// <summary>
/// A page's editing that counts its applies, so a test sees whether a refused move applied nothing, and throws
/// <see cref="Thrown"/> from Apply while it is set, as a bug the page's problem line missed would.
/// </summary>
internal sealed class NotesEditing : ICatalogEditing<NotesCatalog>
{
    public int ApplyCount { get; private set; }

    public Exception? Thrown { get; set; }

    public NotesCatalog Start() => new();

    public void Apply(NotesCatalog edited)
    {
        if (Thrown is not null)
        {
            throw Thrown;
        }

        ApplyCount++;
    }
}
