namespace JRPG.Core.Cutscene.Models
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
