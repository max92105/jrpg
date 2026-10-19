namespace Jrpg.Game.Ui;

/// <summary>
/// The play time as legacy <c>MenuHeader</c> wrote it, hh:mm:ss with hours counting past a day. The text is rebuilt only
/// when the shown second changes, so a header updated every frame formats once a second.
/// </summary>
internal sealed class PlayTimeText
{
    private long _shownSeconds = -1;

    public string Text { get; private set; } = string.Empty;

    public void Update(TimeSpan playTime)
    {
        var seconds = (long)playTime.TotalSeconds;
        if (seconds == _shownSeconds)
        {
            return;
        }

        _shownSeconds = seconds;
        Text = $"{seconds / 3600:00}:{seconds / 60 % 60:00}:{seconds % 60:00}";
    }
}
