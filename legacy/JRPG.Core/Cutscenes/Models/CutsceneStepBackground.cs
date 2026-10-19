namespace JRPG.Core.Cutscenes.Models
{
    public class CutsceneStepBackground : CutsceneStep
    {
        public string BackgroundPath { get; }
        public CutsceneStepBackground(string backgroundPath) => BackgroundPath = backgroundPath;
    }
}
