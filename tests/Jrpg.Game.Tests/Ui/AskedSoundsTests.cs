using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class AskedSoundsTests
{
    private readonly AskedSounds _sounds = new();

    [Fact]
    public void Ask_SameFileTwice_KeepsItOnce()
    {
        _sounds.Ask("a.wav");
        _sounds.AskAll(["a.wav", "b.wav"]);

        Assert.Equal(["a.wav", "b.wav"], _sounds.Files);
    }

    [Fact]
    public void AskMoveIfChanged_SelectionMoved_AsksTheMoveSound()
    {
        _sounds.AskMoveIfChanged(0, 1);

        Assert.Equal([UiSoundFiles.Effects.MenuMove], _sounds.Files);
    }

    [Fact]
    public void AskMoveIfChanged_SelectionStayed_AsksNothing()
    {
        _sounds.AskMoveIfChanged(1, 1);

        Assert.Empty(_sounds.Files);
    }

    [Fact]
    public void Clear_AfterAsking_LeavesNothing()
    {
        _sounds.Ask("a.wav");

        _sounds.Clear();

        Assert.Empty(_sounds.Files);
    }
}
