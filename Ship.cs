using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using ParticleEngine;

namespace Spaceship
{
    class Ship : Entity
    {
        private const float _delay = 2; // seconds
        private float _remainingDelay = _delay;
        private bool fireevent = false;

        public Color[] colors;
        public Color color;
        public int color_index;
        public bool color_changed = false;

        public float health = 100;
        public override Texture2D texture { get; set; }
        public bool IsDead = false;
        public override Vector2 position { get; set; }
        public Vector2 velocity = Vector2.Zero;
        public Vector2 accel = Vector2.Zero;
        public float angle = 0f;
        public float friction = 1f;
        public bool primary_fired = false;
        public Vector2 pos_normalized;
        public Vector2 angle_vector;
        public float scale;
        Random rand = new Random();
        KeyboardState keyboardState;
        MouseState mouseState;
        ParticleManager<ParticleState> testManager;// = new ParticleManager<ParticleState>(200, ParticleState.UpdateParticle, );
        //So how this works is that the PM delegate takes two parameters.
        //The first is how many particles fit in the internal array it holds.
        //The second is a struct containing different ways to update particle behavior.
        //You simply pick one of the public methods within the ParticleState struct.
        //You don't actually call the method here; what this does is tell any particle created through that manager via CreateParticle...
        //...to use the ParticleState.(YourChosenMethod) method to update its properties.

        ParticleManager<ParticleState> primaryweapon_PM;
        ParticleManager<ParticleState> PW_Splash;
        ParticleManager<ParticleState> deathExplosion;

        public Ship(GameRoot root)
        {
            position = new Vector2(100);//new Vector2(rand.Next(root.graphics.PreferredBackBufferWidth), rand.Next(root.graphics.PreferredBackBufferHeight));
            testManager = new ParticleManager<ParticleState>(200, ParticleState.UpdateParticle, ParticleManager<ParticleState>.ParticleAttribute.Inert, root);
            primaryweapon_PM = new ParticleManager<ParticleState>(200, ParticleState.Update_PW_Bullets, ParticleManager<ParticleState>.ParticleAttribute.PlayerBullet, root);
            PW_Splash = new ParticleManager<ParticleState>(200, ParticleState.PW_Splash, ParticleManager<ParticleState>.ParticleAttribute.Inert, root);
            deathExplosion = new ParticleManager<ParticleState>(200, ParticleState.UpdateParticle, ParticleManager<ParticleState>.ParticleAttribute.Inert, root);
            IsDead = false;
            color = Color.Blue;
            color_index = 0;
            colors = new Color[3];
            colors[0] = Color.Blue;
            colors[1] = Color.Green;
            colors[2] = Color.Red;

            //Now look at this. UpdateParticle came with the engine, but now we made a custom update method for the main gun's bullets.
        }

        void Movement()
        //rewrite this such that
        {

            if (keyboardState.IsKeyDown(Keys.T) && !color_changed)
            {
                if (color_index >= colors.Length - 1)
                {
                    color_index = 0;
                    color = colors[color_index];
                }

                else
                {
                    color_index++;
                    color = colors[color_index];
                }

                color_changed = true;
            }

            if (!keyboardState.IsKeyDown(Keys.T))
            {
                color_changed = false;
            }

            keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                angle += 0.1f;

                if (angle >= MathHelper.TwoPi)
                {
                    angle = 0;
                }
            }

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                angle -= 0.1f;

                if (angle <= 0)
                {
                    angle = MathHelper.TwoPi;
                }
            }

            if (keyboardState.IsKeyDown(Keys.Up))
            {
                velocity.X += (float)Math.Cos(angle) * 0.5f;
                velocity.Y += (float)Math.Sin(angle) * 0.5f;
            }

            friction = 1 - velocity.Length() / 50;

            velocity *= friction;
            position += velocity;
        }

        void MovementNew()
        {
            keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Up))
            {
                velocity.X += (float)Math.Cos(angle) * 0.25f;
                velocity.Y += (float)Math.Sin(angle) * 0.25f;
            }

            friction = 1 - velocity.Length() / 500;

            velocity *= friction;
            position += velocity;
        }

        void MouseActions()
        {
            mouseState = Mouse.GetState();
            angle = (float)Math.Atan2(mouseState.Y - position.Y, mouseState.X - position.X);
        }

        void Exhaust(GameRoot root)
        {
            //particleManager.CreateParticle(root.ship, position, Color.White, 200f, 10f, particleState);
            //particleManager.Update();
        }

        public void Kill(GameRoot root)
        {
            for (int i = 0; i < 50; i++)
            {
                var state = new ParticleState()
                {
                    //ParentAngle = (float)rand.NextDouble();
                    AngleVector = new Vector2((float)rand.NextDouble() * rand.Next(-10, 10), (float)rand.NextDouble() * rand.Next(-10, 10)),
                    Type = ParticleType.None,
                    LengthMultiplier = 1f
                };
                deathExplosion.CreateParticle(root.kirby, position, Color.White, 150, 0.1f, state, root);
            }
            IsDead = true;
        }

        public override void Update(GameRoot root, GameTime gameTime)
        {

            pos_normalized = position;
            pos_normalized.Normalize();

            var x = (float)Math.Cos(angle);
            var y = (float)Math.Sin(angle);

            angle_vector = new Vector2(x,y);

            if (keyboardState.IsKeyDown(Keys.A) && !primary_fired)
            {
                primary_fired = true;
                PrimaryWeapon(root);
            }

            if (keyboardState.IsKeyDown(Keys.B))
            {
                testbigexplosion(root);
            }

            if (true)
            {
                //spiral(root,gameTime);
            }

            if (!keyboardState.IsKeyDown(Keys.A))
            {
                primary_fired = false;
            }

            if (health <= 0 && !IsDead)
            {
                Kill(root); //purpose of this "isDead" bool is to ensure the death sequence only gets called once
            }

            if (IsDead)
            {
                deathExplosion.Update();
                if (deathExplosion.ParticleCount == 0)
                {
                    EntityManager.ships.Remove(this); //remove this enemy from the list, which should stop all Update() and Draw() calls
                    //but don't actually delete it until the death explosion fades, because removing the enemy from the EM will also delete the explosion's PM
                }
            }

            Movement();

            if (primaryweapon_PM.CollisionDetected) //if the main gun registers a collision, tell the splashy PM to make the splash effects. 
            {
                for (int i = 0; i < 10; i++)
                {
                    var state = new ParticleState()
                    {
                        //ParentAngle = (float)rand.NextDouble();
                        AngleVector = new Vector2((float)rand.NextDouble() * rand.Next(-3, 3), (float)rand.NextDouble() * rand.Next(-3, 3)),
                        Type = ParticleType.None,
                        LengthMultiplier = 1f
                    };
                    PW_Splash.CreateParticle(root.kirby, primaryweapon_PM.CollisionLocation, Color.White, 150, 0.1f, state, root);
                }
                
                primaryweapon_PM.CollisionDetected = false; //turn off the fireworks after we've finished detonating
            }

            root.testvar = health;
            primaryweapon_PM.Update();
            PW_Splash.Update();
        }

        Vector2 RandomV2(Vector2 min, Vector2 max)
        {
            Random r = new Random();
            return Vector2.Lerp(min, max, (float)r.NextDouble());
        }

        public void PrimaryWeapon(GameRoot root)
        {
            //Vector2.Add()
            var state = new ParticleState()
            {
                ParentAngle = angle,
                AngleVector = angle_vector * 5,// + velocity,
                Type = ParticleType.None,
                LengthMultiplier = 1f
            };

            primaryweapon_PM.CreateParticle(root.circle, position - root.player_origin , color, color, 300, 1f, state, root); //reassign origin to a ship property later
        }

        public void testbigexplosion(GameRoot root)
        {
            for (int i = 0; i < 120; i++)
            {
                //float speed = 18f * (1f - 1 / rand.NextFloat(1f, 10f));
                var state = new ParticleState()
                {
                    AngleVector = new Vector2((float)rand.NextDouble() * rand.Next(-10,10), (float)rand.NextDouble() * rand.Next(-10,10)),
                    Type = ParticleType.None,
                    LengthMultiplier = 1
                };
                Color color = Color.Lerp(Color.Red, Color.White, (float)rand.NextDouble());
                primaryweapon_PM.CreateParticle(root.circle, position - root.player_origin, color, 1900, 1f, state, root);
            }
        }

        public void spiral(GameRoot root, GameTime gameTime)
        {
            var timer = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _remainingDelay -= timer;

            if (_remainingDelay <= 0)
            {
                fireevent = true;
                _remainingDelay = _delay;
            }

            if (fireevent)
            {
                //float speed = 18f * (1f - 1 / rand.NextFloat(1f, 10f));
                var state = new ParticleState()
                {
                    AngleVector = angle_vector,
                    Type = ParticleType.None,
                    LengthMultiplier = 1
                };
                Color color = Color.Lerp(Color.Blue, Color.White, (float)rand.NextDouble());
                testManager.CreateParticle(root.circle, position - root.player_origin, color, 1900, 1f, state, root);
            }

            fireevent = false;
        }

        public override void Draw(GameRoot root, SpriteBatch batch)
        {
                primaryweapon_PM.Draw(batch);
                PW_Splash.Draw(batch);


            if (IsDead)
            {
                deathExplosion.Draw(batch);
            }

        }
    }
}
//Easiest way to give object classes access to the game root is to create two master methods (Draw and Update), call them in the root's own Draw and Update,
//and then pass the root in as a parameter to these master methods. Then put all local methods into the masters, letting them access the passed parameter.
