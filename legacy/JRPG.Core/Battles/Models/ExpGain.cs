using System.Collections.Generic;

namespace JRPG.Core.Battles.Models
{
    /// <summary>The EXP one party member got at the end of a battle, and how it played out level by level.</summary>
    public class ExpGain
    {
        public Character Character { get; set; }
        public int Amount { get; set; }
        public List<ExpStep> Steps { get; set; } = new List<ExpStep>();
    }
}
