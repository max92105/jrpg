using System.Runtime.ExceptionServices;

namespace Jrpg.Editor.Tests;

// WPF imaging needs an STA thread and xUnit's are MTA: the body runs on a thread of its own, and what it throws, a
// failed assert included, is thrown again here with its own stack.
internal static class StaThread
{
    public static void Run(Action body)
    {
        ExceptionDispatchInfo? thrown = null;
        var thread = new Thread(() =>
        {
            try
            {
                body();
            }
            catch (Exception exception)
            {
                thrown = ExceptionDispatchInfo.Capture(exception);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        thrown?.Throw();
    }
}
