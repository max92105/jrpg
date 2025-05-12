namespace JRPG.Core.Cutscene.Models
{
    public class CutsceneStepBackground : CutsceneStep
    {
        public string BackgroundPath { get; }
        public CutsceneStepBackground(string backgroundPath) => BackgroundPath = backgroundPath;
    }
}
