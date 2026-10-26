using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// How the victory screen comes in: the title slides in, then the reward bar fades in, then the cards one after
/// another, and after a pause the bars start filling.
/// </summary>
internal sealed class VictoryEntrance
{
    // Placeholder timing: the title, the reward bar's delay, the first card's delay, the stagger between cards, the
    // pause before the bars fill, and the 0.3 s fades.
    private static readonly TimeSpan _titleTime = TimeSpan.FromSeconds(0.45);
    private static readonly TimeSpan _rewardDelay = TimeSpan.FromSeconds(0.25);
    private static readonly TimeSpan _firstCardDelay = TimeSpan.FromSeconds(0.45);
    private static readonly TimeSpan _cardStagger = TimeSpan.FromSeconds(0.1);
    private static readonly TimeSpan _pauseBeforeBars = TimeSpan.FromSeconds(0.5);
    private static readonly TimeSpan _fadeTime = TimeSpan.FromSeconds(0.3);

    private const float TitleSlide = 60;
    private const float CardSlide = 30;

    private readonly TimeSpan _barsStart;
    private TimeSpan _elapsed;

    /// <param name="cardCount">How many member cards come in.</param>
    public VictoryEntrance(int cardCount) => _barsStart = _firstCardDelay + (_cardStagger * cardCount) + _pauseBeforeBars;

    /// <summary>Whether everything has come in and the bars may fill.</summary>
    public bool IsComplete => _elapsed >= _barsStart;

    /// <summary>How long the bars have been filling: the time past the pause, 0 before it.</summary>
    public TimeSpan BarTime => IsComplete ? _elapsed - _barsStart : TimeSpan.Zero;

    public float TitleOpacity => Easing.ShareOf(_elapsed, _titleTime);

    /// <summary>How far left of its place the title still is, in pixels; it slows down as it arrives.</summary>
    public float TitleOffset => -TitleSlide * (1 - Easing.CubicOut(Easing.ShareOf(_elapsed, _titleTime)));

    public float RewardOpacity => Easing.ShareOf(_elapsed - _rewardDelay, _fadeTime);

    public float CardOpacity(int index) => Easing.ShareOf(_elapsed - CardDelay(index), _fadeTime);

    /// <summary>How far left of its place a card still is, in pixels.</summary>
    public float CardOffset(int index) => -CardSlide * (1 - Easing.CubicOut(Easing.ShareOf(_elapsed - CardDelay(index), _fadeTime)));

    public void Update(TimeSpan delta) => _elapsed += delta;

    public void Skip()
    {
        if (!IsComplete)
        {
            _elapsed = _barsStart;
        }
    }

    private static TimeSpan CardDelay(int index) => _firstCardDelay + (_cardStagger * index);
}
