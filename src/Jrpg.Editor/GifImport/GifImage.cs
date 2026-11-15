namespace Jrpg.Editor.GifImport;

/// <summary>A GIF as read: its canvas size in pixels and its frames in order, each still a patch.</summary>
internal sealed record GifImage(int Width, int Height, IReadOnlyList<GifFrame> Frames);
