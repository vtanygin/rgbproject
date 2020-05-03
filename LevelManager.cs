using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ParticleEngine;

namespace Spaceship
{
    public static class LevelManager
    {
        public static List<Level> Levels;
        public static void InitializeLevels(List<Level> levels)
        {

        }

        public static void GenerateRandomLevel(GameRoot root)
        {
            Random r = new Random();

            for (int i = 0; i < root.level_counter; i++)
            {
                Enemy e = new Enemy(root, Enemy.typelist[i % 3]);
                e.texture = root.triangle_right;
                e.position = new Vector2(r.Next(root.graphics.PreferredBackBufferWidth - e.texture.Width), r.Next(root.graphics.PreferredBackBufferHeight - e.texture.Height));
                e.angle = MathHelper.Clamp((float)r.NextDouble() * 4, 0, MathHelper.TwoPi);
                e.laser_period = MathHelper.Clamp((float)r.NextDouble(), 1/(i+1), 1f); //increase periodicity limit as levels increase
                e.x_increment = e.angle; //- MathHelper.PiOver2;
                e.y_increment = e.angle; //- MathHelper.PiOver2;
                e.base_color = root.colors[i % root.colors.Length];
                EntityManager.Add(e);
            }
        }

    }
}
