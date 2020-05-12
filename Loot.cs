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
        public override float scale { get; set; }
        public bool pickedup;
        public bool stackable;
        public float powerup_timer;
        public bool start_timer;
        public bool temporary;
        public bool expired;
        public float sprite_rotation = 0f;
        public float sprite_scale = 1f;
        public bool scale_up = true;
        public LootType lootType;
        public override Texture2D texture { get; set; }

        public override Vector2 position { get; set; }
        public override Vector2 origin { get; set; }

        public override bool DoDraw { get; set; }

        public enum LootType
        {
            Trishot
        }

        public Loot(GameRoot root, LootType lootType)
        {
            DoDraw = true;
            this.lootType = lootType;
            scale = 1f;

            switch (this.lootType)
            {
                case LootType.Trishot:
                    texture = root.diamond;
                    stackable = false;
                    expired = false;
                    temporary = true;
                    powerup_timer = 5f;
                    break;
                default:
                    break;
            }

            EntityManager.Add(this);
            EntityManager.loots.Add(this);
            
            //var filler = EntityManager.loots;

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
            //The EM handles collisions and anything about the player that must be updated when they pick up the loot.
            //keep it in the EM, so we can keep using the EM to call its Update() while the player is carrying the loot.
            //but we place the loot way offscreen so that the player will never interact with it on the map.
            if (pickedup)
            {
                position = new Vector2(-1000); 
                //DoDraw = false;
                start_timer = true;
            }

            if (start_timer)
            {
                var timer = (float)gameTime.ElapsedGameTime.TotalSeconds;
                powerup_timer -= timer;
            }

            if (powerup_timer <= 0 && !expired)
            {
                ExpireLoot();
                expired = true;
            }

            sprite_rotation += 0.1f;
            PulseSprite();

        }

        private void PulseSprite()
        {
            if (scale_up)
            {
                sprite_scale += 0.01f;
            }

            if (!scale_up)
            {
                sprite_scale -= 0.01f;
            }

            if (sprite_scale >= 1.2f)
            {
                scale_up = false;
            }

            if (sprite_scale <= 0.8f)
            {
                scale_up = true;
            }
        }

        private void ExpireLoot()
        {
            EntityManager.ships[0].pickups.Remove(this);
            EntityManager.loots.Remove(this);

            switch (this.lootType)
            {
                case LootType.Trishot:
                    EntityManager.ships[0].primary_weapon.weaponType = Weapon.WeaponType.Default;
                    break;
                default:
                    break;
            }

            //Now this is some crappy code design that's going to become unwieldy
            //as I add more loot. But it works for now.
        }

        public override void Draw(GameRoot gameRoot, SpriteBatch spriteBatch)
        {
            //The loot sprite itself is drawn by the EM...
            //...so unless we want the loot to have some kind of particle effects, we can just leave this empty.
                if (this.temporary && powerup_timer >= 0 && pickedup)
                {
                    spriteBatch.DrawString(gameRoot.timerfont, "Trishot: " + powerup_timer.ToString(), new Vector2(500, 50), Color.Lerp(Color.White, Color.TransparentBlack, 1 - powerup_timer/5f));
                    spriteBatch.Draw(gameRoot.powerup_bar, new Vector2(500, 100), null, Color.Lerp(Color.White, Color.TransparentBlack, 1 - powerup_timer / 5f), 0f, Vector2.Zero, new Vector2(powerup_timer/5f,0.5f), SpriteEffects.None, 1f);
                }
        }
    }
}
