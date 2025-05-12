using System.Collections.Generic;

namespace JRPG.Core.Cutscene.Models
{
    public class CutsceneStepDialogue : CutsceneStep
    {
        public List<string> Lines { get; }
        public string SpeakerName { get; }
        public string PortraitPath { get; }

        public CutsceneStepDialogue(List<string> lines, string speakerName, string portraitPath)
        {
            Lines = lines;
            SpeakerName = speakerName;
            PortraitPath = portraitPath;
        }
    }
}
