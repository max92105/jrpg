namespace JRPG.Core.Cutscene.Models
{
    public class CutsceneStepFade : CutsceneStep
    {
        public double ToOpacity { get; }
        public double Duration { get; }
        public CutsceneStepFade(double toOpacity, double duration)
        {
            ToOpacity = toOpacity;
            Duration = duration;
        }
    }
}
