namespace Jrpg.Editor.Hosting;

/// <summary>
/// Decides, for each exception nothing caught, whether it is written and whether the developer is asked (ADR 0050). A
/// new error is written and asked about; a different error while the box is open is written without a second box
/// (decision 670); the same error again, by its type and its whole stack, is only counted (decision 690), so a throw in
/// a per-frame tick costs one box and one file. The stack, not only the method that threw: two windows' XAML failures
/// both throw from WPF's loader, and each must get its own report.
/// </summary>
/// <param name="log">Where each new error's report goes.</param>
/// <param name="ask">Shows the box and returns the developer's answer.</param>
internal sealed class UnexpectedErrors(UnexpectedErrorLog log, Func<UnexpectedErrorReport, UnexpectedErrorAnswer> ask)
{
    private readonly Dictionary<(Type Type, string? Stack), ReportedError> _reported = [];
    private bool _isAsking;

    /// <param name="exception">What nothing caught.</param>
    /// <param name="at">When, which names its report.</param>
    /// <returns>Whether the editor goes on or closes; it goes on whenever nobody was asked.</returns>
    public UnexpectedErrorAnswer Handle(Exception exception, DateTimeOffset at)
    {
        var error = (exception.GetType(), exception.StackTrace);
        if (_reported.TryGetValue(error, out var reported))
        {
            reported.Repeats++;
            return UnexpectedErrorAnswer.KeepWorking;
        }

        var report = new UnexpectedErrorReport(exception, log.Write(exception, at));
        _reported.Add(error, new ReportedError(report.File));
        if (_isAsking)
        {
            return UnexpectedErrorAnswer.KeepWorking;
        }

        // Cleared whatever the box does, so a box that fails doesn't silence every later error.
        _isAsking = true;
        try
        {
            return ask(report);
        }
        finally
        {
            _isAsking = false;
        }
    }

    /// <summary>
    /// Adds to each written report how many more times its error came back; called as the editor closes.
    /// </summary>
    public void AddRepeatCounts()
    {
        foreach (var reported in _reported.Values)
        {
            if (reported is { File: { } file, Repeats: > 0 })
            {
                UnexpectedErrorLog.AddRepeatCount(file, reported.Repeats);
            }
        }
    }

    private sealed class ReportedError(string? file)
    {
        public string? File => file;

        public int Repeats { get; set; }
    }
}
