using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleEngine;

namespace Spaceship
{
    public abstract class Entity
    {
        public abstract Vector2 position { get; set; }
        public abstract Texture2D texture { get; set; }

        public abstract void Update(GameRoot gameRoot, GameTime gameTime);

        public abstract void Draw(GameRoot gameRoot, SpriteBatch spriteBatch);

        //how does this work?
        //The idea is that we can throw all objects (enemies, the player, etc.) that inherit from "Entity" into a 
        //single list of "Entity" objects. We can then call Entity.Update or Entity.Draw,
        //and the interpreter will automatically call the type-specific version of the method,
        //instead of literally calling Entity.Update.
        //Same with the properties. If we define a local Entity "myEntity" and refer to its position or texture,
        //the interpreter will retrieve "Ship.position" or "Enemy.position" depending on the child type of the Entity.
    }
}
