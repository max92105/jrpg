namespace Jrpg.Data.Art;

/// <summary>
/// The frame grid every field sprite sheet follows (the player's and NPCs', ADR 0015): square frames, a column per walk
/// frame and a row per facing. The game draws from it and the editor crops its standing frame, so both read it here.
/// </summary>
public static class FieldSpriteLayout
{
    /// <summary>A frame in image pixels, the size of a screen cell only by coincidence (question 58).</summary>
    public const int FramePixels = 48;

    /// <summary>One column per walk frame; the walk animation cycles through them.</summary>
    public const int WalkFrames = 8;

    /// <summary>One row per facing.</summary>
    public const int Rows = 4;

    /// <summary>Whether an image this size holds every frame; a larger one is allowed and its extra pixels unused.</summary>
    public static bool Holds((int Width, int Height) size) => size.Width >= WalkFrames * FramePixels && size.Height >= Rows * FramePixels;
}
