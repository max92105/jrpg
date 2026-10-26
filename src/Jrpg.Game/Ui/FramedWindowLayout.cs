using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// Where a framed window's parts sit around a body of a given size, in the reference UI's frame language: an outer
/// hairline, an inner one inset with open corners, and the body padded inside them (plan 2026-10-25-ui-second-pass,
/// W1a).
/// </summary>
/// <param name="Frame">The outer hairline's rectangle, which is also the window's dark body.</param>
/// <param name="InnerFrame">The inner hairline's rectangle.</param>
/// <param name="Body">Where the caller writes the window's content.</param>
internal readonly record struct FramedWindowLayout(Rectangle Frame, Rectangle InnerFrame, Rectangle Body)
{
    /// <summary>The body's padding inside the outer hairline (placeholder).</summary>
    public const int PaddingX = 32;

    /// <inheritdoc cref="PaddingX"/>
    public const int PaddingY = 24;

    // The inner hairline sits 4 px inside the outer one (placeholder).
    private const int InnerInset = 4;

    /// <summary>A window with no title, centered on <paramref name="center"/>.</summary>
    /// <param name="center">The window's center on the virtual screen.</param>
    /// <param name="bodySize">The content's width and height.</param>
    public static FramedWindowLayout Untitled(Point center, Point bodySize)
    {
        var body = new Rectangle(center.X - (bodySize.X / 2), center.Y - (bodySize.Y / 2), bodySize.X, bodySize.Y);
        var frame = body;
        frame.Inflate(PaddingX, PaddingY);
        var inner = frame;
        inner.Inflate(-InnerInset, -InnerInset);
        return new FramedWindowLayout(frame, inner, body);
    }
}
