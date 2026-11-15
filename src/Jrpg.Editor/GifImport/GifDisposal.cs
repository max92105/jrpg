namespace Jrpg.Editor.GifImport;

/// <summary>
/// What a GIF does with its canvas after a frame. The file's 0 and 1 read as <see cref="Keep"/>, 2 and 3 as named, and
/// the reserved 4 to 7 as <see cref="Keep"/>, as browsers do; so does a frame without a timing block.
/// </summary>
internal enum GifDisposal
{
    Keep,
    ClearToBackground,
    RestorePrevious,
}
