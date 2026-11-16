using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Jrpg.Editor.Hosting;

/// <summary>
/// A window's or a page's work on each frame WPF draws (<see cref="CompositionTarget.Rendering"/>), stopped when it
/// throws. A handler that threw inside the render pass would throw again on every frame, and WPF would paint nothing:
/// the editor stayed open but blank (found at ADR 0050's hand check). So a throw stops this tick, whose animation or
/// preview freezes, and is raised again outside the render pass, where the unexpected-error box takes it once.
/// </summary>
internal sealed class RenderTick
{
    private readonly EventHandler _onRendering;
    private readonly Action<Exception> _report;
    /// <summary>
    /// Whether the work runs on each frame: false before <see cref="Start"/>, after <see cref="Stop"/>, and from the
    /// frame on which the work threw.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <param name="onRendering">The window's per-frame work.</param>
    public RenderTick(EventHandler onRendering)
        : this(onRendering, RaiseOutsideTheRenderPass(Dispatcher.CurrentDispatcher))
    {
    }

    /// <param name="onRendering">The window's per-frame work.</param>
    /// <param name="report">Takes the exception that stopped the tick.</param>
    internal RenderTick(EventHandler onRendering, Action<Exception> report) => (_onRendering, _report) = (onRendering, report);

    /// <summary>Runs the work on every frame from now; nothing more when it already runs.</summary>
    public void Start()
    {
        if (!IsRunning)
        {
            IsRunning = true;
            CompositionTarget.Rendering += OnFrame;
        }
    }

    public void Stop()
    {
        if (IsRunning)
        {
            IsRunning = false;
            CompositionTarget.Rendering -= OnFrame;
        }
    }

    /// <summary>
    /// Runs the work from each time the page loads to the next time it unloads. WPF may raise Loaded twice, which
    /// <see cref="Start"/> ignores while it runs; a page shown again after Unloaded plays again.
    /// </summary>
    public void RunWhileLoaded(FrameworkElement page)
    {
        page.Loaded += (_, _) => Start();
        page.Unloaded += (_, _) => Stop();
    }

    /// <summary>One frame: the work, or the stop and the report when it throws.</summary>
    internal void OnFrame(object? sender, EventArgs e)
    {
        try
        {
            _onRendering(sender, e);
        }
#pragma warning disable CA1031 // Whatever the work throws must not reach the render pass; it is raised again by the report.
        catch (Exception exception)
#pragma warning restore CA1031
        {
            Stop();
            _report(exception);
        }
    }

    // Thrown again as its own dispatcher work, with its first stack, so the editor's handler sees it as any other.
    private static Action<Exception> RaiseOutsideTheRenderPass(Dispatcher dispatcher) =>
        exception => dispatcher.BeginInvoke(() => ExceptionDispatchInfo.Capture(exception).Throw());
}
