using Jrpg.Game.Rendering;

namespace Jrpg.Game.Ui;

/// <summary>What screens write with: the message table (ADR 0010) and the fonts. Built once by <c>Program</c>.</summary>
internal sealed record ScreenText(UiMessages Messages, FontLibrary Fonts);
