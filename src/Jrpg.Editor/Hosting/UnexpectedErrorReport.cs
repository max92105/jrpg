namespace Jrpg.Editor.Hosting;

/// <summary>One unexpected error and where its report went (ADR 0050).</summary>
/// <param name="Exception">What nothing caught.</param>
/// <param name="File">The report's path; null when it couldn't be written.</param>
internal sealed record UnexpectedErrorReport(Exception Exception, string? File);
