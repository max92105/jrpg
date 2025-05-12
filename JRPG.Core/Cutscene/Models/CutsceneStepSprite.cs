using System.Collections.Generic;

namespace JRPG.Core.Cutscene.Models
{
    public class CutsceneStepSprite : CutsceneStep
    {
        public string Id { get; }
        public string SpritePath { get; }
        public double X { get; }
        public double Y { get; }
        public bool Flip { get; }

        public CutsceneStepSprite(string id, string spritePath, double x, double y, bool flip = false)
        {
            Id = id;
            SpritePath = spritePath;
            X = x;
            Y = y;
            Flip = flip;
        }
    }
}
