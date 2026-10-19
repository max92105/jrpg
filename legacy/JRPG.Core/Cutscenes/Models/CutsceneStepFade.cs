namespace JRPG.Core.Cutscenes.Models
{
    public class CutsceneStepFade : CutsceneStep
    {
        public bool FadeToBlack { get; }
        public double Duration { get; }
        public CutsceneStepFade(bool fadeToBlack, double duration)
        {
            FadeToBlack = fadeToBlack;
            Duration = duration;
        }
    }
}
