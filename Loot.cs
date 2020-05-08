using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Spaceship
{
    public class Loot : Entity
    {
        public bool pickedup;
        public bool stackable;
        public LootType lootType;
        public override Texture2D texture { get; set; }

        public override Vector2 position { get; set; }

        public enum LootType
        {
            Trishot
        }

        public Loot(GameRoot root, LootType lootType)
        {
            this.lootType = lootType;

            switch (this.lootType)
            {
                case LootType.Trishot:
                    texture = root.diamond;
                    stackable = false;
                    break;
                default:
                    break;
            }

            EntityManager.Add(this);
        }

        public static bool sameLootType(Loot a, Loot b)
        {
            if (a.lootType == b.lootType)
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        public override void Update(GameRoot gameRoot, GameTime gameTime)
        {
            //Loot-player collision detection is handled by EM.HandleCollisions().
            if (pickedup)
            {
                EntityManager.loots.Remove(this);
            }

        }

        public override void Draw(GameRoot gameRoot, SpriteBatch spriteBatch)
        {
            //The loot sprite itself is drawn by the EM...
            //...so unless we want the loot to have some kind of particle effects, we can just leave this empty.
        }
    }
}
