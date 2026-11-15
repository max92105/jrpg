namespace Jrpg.Editor.Hosting;

/// <summary>What the developer answers in the unexpected-error box (ADR 0050).</summary>
internal enum UnexpectedErrorAnswer
{
    /// <summary>The editor stays open with its draft.</summary>
    KeepWorking,

    /// <summary>The editor closes through its usual close question.</summary>
    CloseEditor,
}
