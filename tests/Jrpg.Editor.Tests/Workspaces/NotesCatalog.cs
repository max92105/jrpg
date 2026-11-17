using System.ComponentModel;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Tests.Workspaces;

/// <summary>A page's edited catalog with nothing but what the workspace reads, its problem set by the test.</summary>
internal sealed class NotesCatalog : IEditedCatalog
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string? Problem
    {
        get;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Problem)));
        }
    }

    public string? ChosenKey { get; private set; }

    public void Choose(string key) => ChosenKey = key;
}
