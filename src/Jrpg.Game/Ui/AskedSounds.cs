namespace Jrpg.Game.Ui;

/// <summary>The effect files one update of a scene or page asks for, cleared as the next starts (ADR 0035).</summary>
internal sealed class AskedSounds
{
    private readonly List<string> _files = [];

    public IReadOnlyList<string> Files => _files;

    public void Ask(string file)
    {
        if (!_files.Contains(file))
        {
            _files.Add(file);
        }
    }

    public void AskAll(IReadOnlyList<string> files)
    {
        for (var index = 0; index < files.Count; index++)
        {
            Ask(files[index]);
        }
    }

    /// <summary>The move sound when the selection really moved, so a list of one stays silent.</summary>
    public void AskMoveIfChanged(int selectedBefore, int selectedAfter)
    {
        if (selectedBefore != selectedAfter)
        {
            Ask(UiSoundFiles.Effects.MenuMove);
        }
    }

    public void Clear() => _files.Clear();
}
