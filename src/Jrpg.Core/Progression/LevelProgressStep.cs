namespace Jrpg.Core.Progression;

/// <summary>One level's worth of an experience gain, as the victory screen fills the bar.</summary>
public sealed record LevelProgressStep(int Level, int FromExperience, int ToExperience, int ExperienceToNextLevel)
{
    public bool IsLevelUp => ToExperience == ExperienceToNextLevel;
}
