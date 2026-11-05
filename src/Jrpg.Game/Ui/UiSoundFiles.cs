using Jrpg.Data.Audio;

namespace Jrpg.Game.Ui;

/// <summary>
/// The sound files the code plays that no content file names, so the game and a test name them in one place (decision
/// 425). Grouped by the audio folder they're in.
/// </summary>
internal static class UiSoundFiles
{
    /// <summary>In <see cref="AudioFolders.Music"/>: each track's loop, in its own folder (decision 496).</summary>
    public static class Music
    {
        public const string Title = "titlescreen/loop.wav";

        public const string Battle = "battle/loop.wav";
    }

    /// <summary>
    /// In <see cref="AudioFolders.Effects"/>: the interface's sounds (decision 421), and a deal or a salvage gone
    /// through (decision 435).
    /// </summary>
    public static class Effects
    {
        public const string MenuMove = "PlaceholderMenuMove.wav";

        public const string MenuConfirm = "PlaceholderMenuConfirm.wav";

        public const string MenuCancel = "PlaceholderMenuCancel.wav";

        public const string TextTyping = "PlaceholderTextTyping.wav";

        public const string Deal = "PlaceholderDeal.wav";
    }

    /// <summary>
    /// In <see cref="AudioFolders.Effects"/>: what the field's events sound like (decisions 430, 433, 434).
    /// </summary>
    public static class Field
    {
        public const string ItemGot = "PlaceholderItemGot.wav";

        public const string Recruit = "PlaceholderRecruit.wav";

        public const string QuestUpdated = "PlaceholderQuestUpdated.wav";

        public const string QuestCompleted = "PlaceholderQuestCompleted.wav";

        public const string Warp = "PlaceholderWarp.wav";

        public const string Rest = "PlaceholderRest.wav";
    }

    /// <summary>
    /// In <see cref="AudioFolders.Effects"/>: what a battle's start, events and outcomes sound like (decision 423).
    /// </summary>
    public static class Battle
    {
        public const string Hit = "PlaceholderHit.wav";

        public const string Critical = "PlaceholderCritical.wav";

        public const string Miss = "PlaceholderMiss.wav";

        public const string Block = "PlaceholderBlock.wav";

        public const string Resisted = "PlaceholderResisted.wav";

        public const string StatusApplied = "PlaceholderStatusApplied.wav";

        public const string Heal = "PlaceholderHeal.wav";

        public const string Fall = "PlaceholderFall.wav";

        public const string PoisonTick = "PlaceholderPoisonTick.wav";

        public const string RegenTick = "PlaceholderRegenTick.wav";

        public const string Backfire = "PlaceholderBackfire.wav";

        public const string Stunned = "PlaceholderStunned.wav";

        public const string ResourceGain = "PlaceholderResourceGain.wav";

        public const string StatusEnded = "PlaceholderStatusEnded.wav";

        public const string Move = "PlaceholderMove.wav";

        public const string ExtraTurn = "PlaceholderExtraTurn.wav";

        public const string ItemUse = "PlaceholderItemUse.wav";

        public const string Defend = "PlaceholderDefend.wav";

        public const string Enraged = "PlaceholderEnraged.wav";

        public const string Start = "PlaceholderBattleStart.wav";

        public const string Victory = "PlaceholderVictory.wav";

        public const string Defeat = "PlaceholderDefeat.wav";

        public const string Escape = "PlaceholderEscape.wav";

        public const string LevelUp = "PlaceholderLevelUp.wav";
    }
}
