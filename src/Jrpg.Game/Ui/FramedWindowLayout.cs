using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// Where a framed window's parts sit around a body of a given size, in the reference UI's frame language: an outer
/// hairline, an inner one inset with open corners, the body padded inside them, and for a titled window, a title over a
/// separator line (plan 2026-10-25-ui-second-pass, W1a and W1b).
/// </summary>
/// <param name="Frame">The outer hairline's rectangle, which is also the window's dark body.</param>
/// <param name="InnerFrame">The inner hairline's rectangle.</param>
/// <param name="Body">Where the caller writes the window's content.</param>
/// <param name="HasTitle">Whether the window has a title and its separator.</param>
/// <param name="TitleCenter">The middle of the title's line; unused without a title.</param>
/// <param name="SeparatorY">The separator's row, between the title and the body; unused without a title.</param>
internal readonly record struct FramedWindowLayout(
    Rectangle Frame,
    Rectangle InnerFrame,
    Rectangle Body,
    bool HasTitle,
    Vector2 TitleCenter,
    int SeparatorY)
{
    /// <summary>The body's padding inside the outer hairline (placeholder).</summary>
    public const int PaddingX = 32;

    /// <inheritdoc cref="PaddingX"/>
    public const int PaddingY = 24;

    // The inner hairline sits 4 px inside the outer one; the separator 14 px under the title and above the body
    // (placeholders).
    private const int InnerInset = 4;
    private const int SeparatorGap = 14;

    /// <summary>A window with no title, centered on <paramref name="center"/>.</summary>
    /// <param name="center">The window's center on the virtual screen.</param>
    /// <param name="bodySize">The content's width and height.</param>
    public static FramedWindowLayout Untitled(Point center, Point bodySize)
    {
        var frame = new Rectangle(center.X - (bodySize.X / 2) - PaddingX, center.Y - (bodySize.Y / 2) - PaddingY, bodySize.X + (2 * PaddingX), bodySize.Y + (2 * PaddingY));
        var body = new Rectangle(frame.X + PaddingX, frame.Y + PaddingY, bodySize.X, bodySize.Y);
        return new FramedWindowLayout(frame, InnerOf(frame), body, HasTitle: false, TitleCenter: default, SeparatorY: 0);
    }

    /// <summary>A window with a title over a separator, then the body, centered on <paramref name="center"/>.</summary>
    /// <param name="center">The window's center on the virtual screen.</param>
    /// <param name="bodySize">The content's width and height, under the separator.</param>
    /// <param name="titleLineHeight">The title font's line height.</param>
    public static FramedWindowLayout Titled(Point center, Point bodySize, float titleLineHeight)
    {
        var titleHeight = (int)Math.Ceiling(titleLineHeight);
        var height = (2 * PaddingY) + titleHeight + (2 * SeparatorGap) + 1 + bodySize.Y;
        var frame = new Rectangle(center.X - (bodySize.X / 2) - PaddingX, center.Y - (height / 2), bodySize.X + (2 * PaddingX), height);
        var titleTop = frame.Y + PaddingY;
        var separatorY = titleTop + titleHeight + SeparatorGap;
        var body = new Rectangle(frame.X + PaddingX, separatorY + 1 + SeparatorGap, bodySize.X, bodySize.Y);
        var titleCenter = new Vector2(frame.Center.X, titleTop + (titleHeight / 2f));
        return new FramedWindowLayout(frame, InnerOf(frame), body, HasTitle: true, titleCenter, separatorY);
    }

    private static Rectangle InnerOf(Rectangle frame)
    {
        var inner = frame;
        inner.Inflate(-InnerInset, -InnerInset);
        return inner;
    }
}
