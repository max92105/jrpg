namespace Jrpg.Core.Dialogue;

/// <summary>What one speaker says, one message box per line. Speaker and portrait are optional (developer decision 32).</summary>
public sealed record Speech(IReadOnlyList<string> Lines, string? Speaker = null, string? PortraitFile = null);
