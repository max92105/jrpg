namespace Jrpg.Core.Dialogue;

/// <summary>What one speaker says, one message box per line. Speaker and portrait are optional (developer decision 32).</summary>
public sealed record Speech(string? Speaker, string? PortraitFile, IReadOnlyList<string> Lines);
