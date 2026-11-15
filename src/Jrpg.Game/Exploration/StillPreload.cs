using Jrpg.Core.Cinematics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// The pictures a cinematic still needs loaded, one asked per frame in order (decision 628): six full-screen stills
/// loaded in one frame would stall the game and eat the black before the first.
/// </summary>
/// <param name="stills">The cinematic's stills in order; a picture shown twice is asked twice and cached once.</param>
internal sealed class StillPreload(IReadOnlyList<CinematicStill> stills)
{
    private int _loaded;

    /// <summary>The picture to load this frame, under the cinematics folder; null once all are loaded.</summary>
    public string? Next => IsDone ? null : stills[_loaded].ImageFile;

    public bool IsDone => _loaded >= stills.Count;

    /// <summary>The picture <see cref="Next"/> named is loaded; the next one is asked on the next frame.</summary>
    public void MarkLoaded() => _loaded++;
}
