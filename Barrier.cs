using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using ParticleEngine;

namespace Spaceship
{
    public class Barrier : Entity
    {
        public override bool DoDraw { get; set ; }
        public override Vector2 position { get; set; }

        public override float scale { get; set; }
        public override Texture2D texture { get; set; }

        public override Vector2 origin { get; set; }

        public override void Draw(GameRoot gameRoot, SpriteBatch spriteBatch)
        {
            
        }

        public override void Update(GameRoot gameRoot, GameTime gameTime)
        {
            
        }

        public enum BarrierType
        {

        }

        public Barrier(GameRoot root)
        {
            DoDraw = true;
            position = new Vector2(500,200);
            texture = root.hexagon;
            scale = 0.05f;
            origin = new Vector2(512);
        }

    }
}
