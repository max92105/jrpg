using Jrpg.Core.Progression;

namespace Jrpg.Game.Battle;

/// <summary>
/// One member's experience bar on the victory screen, filling through the level steps their gain produced, one after
/// the other.
/// </summary>
internal sealed class ExperienceBarAnimation
{
    // Placeholder timing: a full level fills in 1.1 s, and no step is quicker than 0.2 s so a small gain still shows.
    private static readonly TimeSpan _fullLevelTime = TimeSpan.FromSeconds(1.1);
    private static readonly TimeSpan _minimumStepTime = TimeSpan.FromSeconds(0.2);

    private readonly IReadOnlyList<LevelProgressStep> _steps;
    private readonly int _finalLevel;
    private int _stepIndex;
    private TimeSpan _inStep;

    /// <param name="steps">The gain's steps, in order, as Core produced them.</param>
    /// <param name="finalLevel">The member's level after the gain, shown once the bar is full.</param>
    public ExperienceBarAnimation(IReadOnlyList<LevelProgressStep> steps, int finalLevel)
    {
        _steps = steps;
        _finalLevel = finalLevel;
    }

    public bool IsComplete => _stepIndex >= _steps.Count;

    /// <summary>The level the bar is filling: the step's, then the member's final level once complete.</summary>
    public int Level => IsComplete ? _finalLevel : _steps[_stepIndex].Level;

    /// <summary>
    /// The experience shown into <see cref="Level"/>; 0 with no steps, and 0 when the gain ended exactly on a level-up,
    /// since Core then starts the new level empty and adds no step for it.
    /// </summary>
    public int Experience
    {
        get
        {
            if (IsComplete)
            {
                return _steps.Count == 0 || _steps[^1].IsLevelUp ? 0 : _steps[^1].ToExperience;
            }

            var step = _steps[_stepIndex];
            var share = _inStep / DurationOf(step);
            return step.FromExperience + (int)((step.ToExperience - step.FromExperience) * share);
        }
    }

    /// <summary>The experience <see cref="Level"/> needs; 0 with no steps.</summary>
    public int ExperienceToNextLevel => IsComplete
        ? _steps.Count == 0 ? 0 : _steps[^1].ExperienceToNextLevel
        : _steps[_stepIndex].ExperienceToNextLevel;

    /// <summary>How many level-ups the bar has filled so far; "Level Up!" shows from the first.</summary>
    public int LevelUps { get; private set; }

    /// <summary>
    /// Time since the bar last filled a level, for the pop; null before any level-up, and after a skip, which shows
    /// "Level Up!" without popping.
    /// </summary>
    public TimeSpan? SinceLastLevelUp { get; private set; }

    public void Update(TimeSpan delta)
    {
        _inStep += delta;
        SinceLastLevelUp += delta;
        while (!IsComplete && _inStep >= DurationOf(_steps[_stepIndex]))
        {
            _inStep -= DurationOf(_steps[_stepIndex]);
            FinishStep();
        }
    }

    public void Skip()
    {
        while (!IsComplete)
        {
            FinishStep();
        }

        SinceLastLevelUp = null;
    }

    private static TimeSpan DurationOf(LevelProgressStep step)
    {
        var levelShare = (double)(step.ToExperience - step.FromExperience) / step.ExperienceToNextLevel;
        var duration = _fullLevelTime * levelShare;
        return duration > _minimumStepTime ? duration : _minimumStepTime;
    }

    private void FinishStep()
    {
        if (_steps[_stepIndex].IsLevelUp)
        {
            LevelUps++;
            SinceLastLevelUp = TimeSpan.Zero;
        }

        _stepIndex++;
    }
}
