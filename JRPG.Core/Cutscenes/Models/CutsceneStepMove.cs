namespace JRPG.Core.Cutscenes.Models
{
    public class CutsceneStepMove : CutsceneStep
    {
        public string Id { get; }
        public double ToX { get; }
        public double ToY { get; }
        public double Duration { get; }

        public CutsceneStepMove(string id, double toX, double toY, double duration)
        {
            Id = id;
            ToX = toX;
            ToY = toY;
            Duration = duration;
        }
    }
}
