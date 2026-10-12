namespace JRPG.Enums
{
    /// <summary>Where the game goes once a battle is over.</summary>
    public enum BattleExit
    {
        /// <summary>Victory or escape: back to the map.</summary>
        BackToMap,

        /// <summary>Defeat: title screen, with the Load list open.</summary>
        LoadSave,

        /// <summary>Defeat: title screen.</summary>
        TitleScreen
    }
}
