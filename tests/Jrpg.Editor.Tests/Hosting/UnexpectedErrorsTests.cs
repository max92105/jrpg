using Jrpg.Editor.Hosting;

namespace Jrpg.Editor.Tests.Hosting;

// The unexpected-error policy (ADR 0050): what is written and when the developer is asked, over a log in a temporary
// folder; each exception is thrown and caught in its own method, so it carries a stack of its own.
public sealed class UnexpectedErrorsTests : IDisposable
{
    private static readonly DateTimeOffset _at = new(2026, 11, 15, 10, 30, 0, TimeSpan.Zero);

    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"jrpg-editor-errors-{Guid.NewGuid():N}");
    private readonly UnexpectedErrorLog _log;
    private readonly List<UnexpectedErrorReport> _asked = [];

    public UnexpectedErrorsTests() => _log = new UnexpectedErrorLog(_folder, "content", TextWriter.Null);

    public void Dispose()
    {
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, recursive: true);
        }
    }

    [Fact]
    public void Handle_AnError_WritesThenAsksWithTheReport()
    {
        var wasWrittenWhenAsked = false;
        var errors = new UnexpectedErrors(_log, report =>
        {
            wasWrittenWhenAsked = File.Exists(report.File);
            return UnexpectedErrorAnswer.KeepWorking;
        });

        errors.Handle(Drawing(), _at);

        Assert.True(wasWrittenWhenAsked);
    }

    [Fact]
    public void Handle_DeveloperKeepsWorking_ReturnsKeepWorking()
    {
        Assert.Equal(UnexpectedErrorAnswer.KeepWorking, Answering(UnexpectedErrorAnswer.KeepWorking).Handle(Drawing(), _at));
    }

    [Fact]
    public void Handle_DeveloperCloses_ReturnsCloseEditor()
    {
        Assert.Equal(UnexpectedErrorAnswer.CloseEditor, Answering(UnexpectedErrorAnswer.CloseEditor).Handle(Drawing(), _at));
    }

    [Fact]
    public void Handle_OtherErrorWhileAsking_WritesWithoutAsking()
    {
        UnexpectedErrors? errors = null;
        var questions = 0;
        errors = new UnexpectedErrors(_log, _ =>
        {
            questions++;
            errors!.Handle(Saving(), _at);
            return UnexpectedErrorAnswer.KeepWorking;
        });

        errors.Handle(Drawing(), _at);

        Assert.Equal((1, 2), (questions, Directory.GetFiles(_folder).Length));
    }

    [Fact]
    public void Handle_SameErrorAfterAnswer_DoesNotAskAgain()
    {
        var errors = Answering(UnexpectedErrorAnswer.KeepWorking);
        errors.Handle(Drawing(), _at);

        errors.Handle(Drawing(), _at);

        Assert.Single(_asked);
    }

    [Fact]
    public void Handle_SameErrorAfterAnswer_WritesNoSecondReport()
    {
        var errors = Answering(UnexpectedErrorAnswer.KeepWorking);
        errors.Handle(Drawing(), _at);

        errors.Handle(Drawing(), _at);

        Assert.Single(Directory.GetFiles(_folder));
    }

    // The render tick's case: the same throw comes back every frame while the box is still open.
    [Fact]
    public void Handle_SameErrorWhileAsking_WritesNoSecondReport()
    {
        UnexpectedErrors? errors = null;
        var questions = 0;
        errors = new UnexpectedErrors(_log, _ =>
        {
            questions++;
            errors!.Handle(Drawing(), _at);
            return UnexpectedErrorAnswer.KeepWorking;
        });

        errors.Handle(Drawing(), _at);

        Assert.Equal((1, 1), (questions, Directory.GetFiles(_folder).Length));
    }

    [Fact]
    public void Handle_SameTypeFromAnotherMethod_Asks()
    {
        var errors = Answering(UnexpectedErrorAnswer.KeepWorking);
        errors.Handle(Drawing(), _at);

        errors.Handle(Saving(), _at);

        Assert.Equal(2, _asked.Count);
    }

    // Two bugs raised through one shared method, as two windows' XAML failures are through WPF's loader.
    [Fact]
    public void Handle_SameThrowingMethodReachedTwoWays_AsksForEach()
    {
        var errors = Answering(UnexpectedErrorAnswer.KeepWorking);
        errors.Handle(LoadingTheShops(), _at);

        errors.Handle(LoadingTheQuests(), _at);

        Assert.Equal(2, _asked.Count);
    }

    [Fact]
    public void Handle_AskThrows_AsksTheNextOne()
    {
        var questions = 0;
        var errors = new UnexpectedErrors(_log, _ =>
        {
            questions++;
            return questions == 1 ? throw new InvalidOperationException("The box failed.") : UnexpectedErrorAnswer.KeepWorking;
        });
        Assert.Throws<InvalidOperationException>(() => errors.Handle(Drawing(), _at));

        errors.Handle(Saving(), _at);

        Assert.Equal(2, questions);
    }

    [Fact]
    public void AddRepeatCounts_SameErrorThreeTimes_AddsTwoMoreToItsReport()
    {
        var errors = Answering(UnexpectedErrorAnswer.KeepWorking);
        errors.Handle(Drawing(), _at);
        errors.Handle(Drawing(), _at);
        errors.Handle(Drawing(), _at);

        errors.AddRepeatCounts();

        var report = File.ReadAllText(_asked[0].File!);
        Assert.EndsWith($"Came back 2 more times.{Environment.NewLine}", report, StringComparison.Ordinal);
    }

    [Fact]
    public void AddRepeatCounts_SameErrorTwice_SaysOnceMore()
    {
        var errors = Answering(UnexpectedErrorAnswer.KeepWorking);
        errors.Handle(Drawing(), _at);
        errors.Handle(Drawing(), _at);

        errors.AddRepeatCounts();

        var report = File.ReadAllText(_asked[0].File!);
        Assert.EndsWith($"Came back once more.{Environment.NewLine}", report, StringComparison.Ordinal);
    }

    [Fact]
    public void AddRepeatCounts_ErrorSeenOnce_LeavesItsReportAsWritten()
    {
        var errors = Answering(UnexpectedErrorAnswer.KeepWorking);
        errors.Handle(Drawing(), _at);
        var written = File.ReadAllText(_asked[0].File!);

        errors.AddRepeatCounts();

        Assert.Equal(written, File.ReadAllText(_asked[0].File!));
    }

    private UnexpectedErrors Answering(UnexpectedErrorAnswer answer) =>
        new(_log, report =>
        {
            _asked.Add(report);
            return answer;
        });

    // Two methods throwing the same type: the policy tells them apart by their stacks.
    private static InvalidOperationException Drawing()
    {
        try
        {
            throw new InvalidOperationException("The drawing failed.");
        }
        catch (InvalidOperationException e)
        {
            return e;
        }
    }

    private static InvalidOperationException Saving()
    {
        try
        {
            throw new InvalidOperationException("The saving failed.");
        }
        catch (InvalidOperationException e)
        {
            return e;
        }
    }

    private static InvalidOperationException LoadingTheShops()
    {
        try
        {
            Load();
            throw new InvalidOperationException("Unreachable.");
        }
        catch (InvalidOperationException e)
        {
            return e;
        }
    }

    private static InvalidOperationException LoadingTheQuests()
    {
        try
        {
            Load();
            throw new InvalidOperationException("Unreachable.");
        }
        catch (InvalidOperationException e)
        {
            return e;
        }
    }

    // The shared method both loads throw from.
    private static void Load() => throw new InvalidOperationException("The loading failed.");
}
