namespace Jrpg.Game.Audio;

/// <summary>
/// The sound effects to play after this update, by file in the effects folder, each once (ADR 0035): many sounds at
/// once can fail to start, and one file twice in one update would only sound louder.
/// </summary>
internal sealed class EffectChannel
{
    private readonly List<string> _played = [];

    public IReadOnlyList<string> Played => _played;

    public void Play(string file)
    {
        if (!_played.Contains(file))
        {
            _played.Add(file);
        }
    }

    public void PlayAll(IReadOnlyList<string> files)
    {
        for (var index = 0; index < files.Count; index++)
        {
            Play(files[index]);
        }
    }

    public void Clear() => _played.Clear();
}
