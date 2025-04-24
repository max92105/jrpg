namespace JRPG
{
    public class Enemy : Fighter
    {
        public string SpritePath { get; set; }

        public Enemy(string name, int hp, int atk, int def, string spritePath)
            : base(name, hp, atk, def)
        {
            SpritePath = spritePath;
        }
    }
}
