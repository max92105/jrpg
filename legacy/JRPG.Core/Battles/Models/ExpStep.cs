namespace JRPG.Core.Battles.Models
{
    /// <summary>
    /// One level's worth of an EXP gain: the bar went from FromEXP to ToEXP (out of EXPToNextLevel)
    /// while at Level. Lets the UI fill the bar level by level.
    /// </summary>
    public class ExpStep
    {
        public int Level { get; set; }
        public int FromEXP { get; set; }
        public int ToEXP { get; set; }
        public int EXPToNextLevel { get; set; }
        public bool LeveledUp { get; set; }
    }
}
