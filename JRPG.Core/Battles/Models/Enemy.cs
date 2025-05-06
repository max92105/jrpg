namespace JRPG.Core.Battles.Models
{
    public class Enemy : Fighter
    {
        public string SpritePath { get; set; }
        public string PortraitPath { get; set; }


        public Enemy(string name, int hp, int atk, int def, string spritePath, string portraitPath)
            : base(name, hp, atk, def)
        {
            SpritePath = spritePath;
            PortraitPath = portraitPath;
        }
    }
}
