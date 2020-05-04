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
    class Enemy : Entity
    {

        public SoundEffectInstance death_sound;
        bool death_sound_played = false;

        private float _delay = 0.2f; // seconds
        private float _remainingDelay; //= _delay;
        private bool fireevent = false;

        private float _laserdelay = 1f;
        private float _laserremainingDelay;
        private bool firingmahlaser = false;
        public float laser_period = 1f;

        public float angle = 0;//MathHelper.PiOver2;
        Random r;
        Random r2;
        public float x_increment = 0;
        public float y_increment = 0;

        public override Texture2D texture { get; set; }
        public EnemyType enemyType;
        public bool IsDead = false;

        public Vector2 last_position;
        public override Vector2 position { get; set; }
        public Vector2 velocity;
        public Vector2 accel;
        public float friction = 0.99f;
        public float rotation_speed = 0f;
        public float rotation_friction = 0.9f;

        //public Vector2 angle_vector;
        public Color[] colors;
        public Color body_color;
        public Color base_color;
        public float health = 100;
        ParticleManager<ParticleState> testManager;
        ParticleManager<ParticleState> deathExplosion;
        ParticleManager<ParticleState> PW_Splash;

        public Enemy(GameRoot gameRoot, EnemyType enemyType) //number one
        {
            deathExplosion = new ParticleManager<ParticleState>(200, ParticleState.UpdateParticle, ParticleManager<ParticleState>.ParticleAttribute.Inert, gameRoot);
            testManager = new ParticleManager<ParticleState>(200, ParticleState.UpdateParticle, ParticleManager<ParticleState>.ParticleAttribute.EnemyBullet, gameRoot);
            PW_Splash = new ParticleManager<ParticleState>(200, ParticleState.PW_Splash, ParticleManager<ParticleState>.ParticleAttribute.Inert, gameRoot);
            r = new Random();
            r2 = new Random();
            position = new Vector2(r.Next(gameRoot.graphics.PreferredBackBufferWidth), r2.Next(gameRoot.graphics.PreferredBackBufferHeight));
            IsDead = false;
            //_delay = 1.5f;
            body_color = Color.White;
            this.enemyType = enemyType;

            colors = new Color[3];
            colors[0] = Color.Blue;
            colors[1] = Color.Green;
            colors[2] = Color.Red;
            //base_color = colors[r.Next(0, 2)];

            velocity = Vector2.Zero;
            accel = Vector2.One;

            if (enemyType == EnemyType.Seeker)
            {
                health = 50; //since they kill the player on contact, make them frailer than baseline so they're not quite as scary to deal with.
            }
        }

        public enum EnemyType
        {
            SlowSpiral,
            Pinwheel,
            Seeker,
        }

        public static EnemyType[] typelist
        {
            get 
            {
                //EnemyType[] list = new EnemyType[Enum.GetValues(typeof(EnemyType)).Length];
                return (EnemyType[])Enum.GetValues(typeof(EnemyType)); //casting EnemyType to Array and back. Seems wasteful but it works.
            } 
            
            private set
            {

            }
        }

        public void Kill(GameRoot root)
        {
            for (int i = 0; i < 50; i++)
            {
                var state = new ParticleState()
                {
                    //ParentAngle = (float)rand.NextDouble();
                    AngleVector = new Vector2((float)r.NextDouble() * r.Next(-10, 10), (float)r.NextDouble() * r.Next(-10, 10)),
                    Type = ParticleType.None,
                    LengthMultiplier = 1f
                };
                deathExplosion.CreateParticle(root.kirby, position + new Vector2(texture.Width/2f), Color.White, 150, 0.1f, state, root);
            }
            IsDead = true;
        }

        public override void Update(GameRoot gameRoot, GameTime gameTime)
        {
            if (gameRoot.enemy_death_sfx != null && death_sound == null)
            {
                death_sound = gameRoot.enemy_death_sfx.CreateInstance();
                // gun_loop.IsLooped = true;
            }

            if (health <= 0 && !IsDead)
            {
                Kill(gameRoot); //purpose of this "isDead" bool is to ensure the death sequence only gets called once
                //isDead is handy because we can use it to stop all Draw and Update calls for the Enemy object itself,
                //but persist any particles/other stuff it has spawned until they expire. Only then do we actually
                //remove it from the EntityManager.
                //gameRoot.testvar += 1;
            }

            if (IsDead)
            {
                if (!death_sound_played)
                {
                    death_sound.Play();
                    death_sound_played = true;
                }

                deathExplosion.Update();
                if (deathExplosion.ParticleCount == 0)
                {
                    EntityManager.enemies.Remove(this); //remove this enemy from the list, which should stop all Update() and Draw() calls
                    //but don't actually delete it until the death explosion fades, because removing the enemy from the EM will also delete the explosion's PM
                }
            }

            if (!IsDead && enemyType == EnemyType.SlowSpiral) //just in case we kill the enemy but somehow don't remove it from the EM
            {
                spiral(gameRoot, gameTime);
            }

            if (!IsDead && enemyType == EnemyType.Pinwheel)
            {
                laser(gameRoot, gameTime);
            }

            if (!IsDead && enemyType == EnemyType.Seeker)
            {
                seeker(gameRoot, gameTime);
            }

            if (testManager.CollisionDetected) //if the main gun registers a collision, tell the splashy PM to make the splash effects. 
            {
                for (int i = 0; i < 10; i++)
                {
                    var state = new ParticleState()
                    {
                        //ParentAngle = (float)rand.NextDouble();
                        AngleVector = new Vector2((float)r.NextDouble() * r.Next(-3, 3), (float)r.NextDouble() * r.Next(-3, 3)),
                        Type = ParticleType.None,
                        LengthMultiplier = 1f
                    };
                    PW_Splash.CreateParticle(gameRoot.kirby, testManager.CollisionLocation, Color.White, 150, 0.1f, state, gameRoot);
                }

                testManager.CollisionDetected = false; //turn off the fireworks after we've finished detonating
            }

            body_color = Color.Lerp(base_color, Color.White, 0.01f*health);
            testManager.Update();
            PW_Splash.Update();
        }

        public override void Draw(GameRoot root, SpriteBatch batch)
        {
                testManager.Draw(batch);
                PW_Splash.Draw(batch);
                //batch.Draw(root.asteroid, position, Color.White);

            if (IsDead)
            {
                deathExplosion.Draw(batch);
            }
        }

        public void spiral(GameRoot root, GameTime gameTime)
        {
            //_delay = 0.1f*(float)Math.Sin(x_increment);
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
                    AngleVector = new Vector2((float)Math.Cos(x_increment) * 3, (float)Math.Sin(y_increment) * 3),
                    //AngleVector = new Vector2((float)Math.Cos(x_increment)*r.Next(1,5), (float)Math.Sin(y_increment)*r.Next(1,5)),
                    Type = ParticleType.None,
                    LengthMultiplier = 1
                };
                x_increment += 0.1f;
                y_increment += 0.1f;
                angle += 0.1f;
                Color color = Color.Lerp(base_color, Color.White, (float)r.NextDouble());
                testManager.CreateParticle(root.circle, position + new Vector2(root.triangle.Width/2,root.triangle.Height/2), color, base_color, 90, 0.4f, state, root);
            }

            fireevent = false;
        }

        public void laser(GameRoot root, GameTime gameTime)
        {
            var timer = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _laserremainingDelay -= timer;

            if (_laserremainingDelay <= 0)
            {
                firingmahlaser = true;
                _laserremainingDelay = _laserdelay;
            }

            if (firingmahlaser)
            {
                //float speed = 18f * (1f - 1 / rand.NextFloat(1f, 10f));
                var state = new ParticleState()
                {
                    AngleVector = new Vector2((float)Math.Cos(x_increment) * 3, (float)Math.Sin(y_increment) * 3),
                    //AngleVector = new Vector2((float)Math.Cos(x_increment)*r.Next(1,5), (float)Math.Sin(y_increment)*r.Next(1,5)),
                    Type = ParticleType.None,
                    LengthMultiplier = 10
                };
                x_increment += laser_period * MathHelper.PiOver2 + 0.01f;
                y_increment += laser_period * MathHelper.PiOver2 + 0.01f;
                angle += laser_period * MathHelper.PiOver2 + 0.01f;
                Color color = Color.Lerp(base_color, Color.White, (float)r.NextDouble());
                testManager.CreateParticle(root.circle, position + new Vector2(root.triangle.Width / 2, root.triangle.Height / 2), color, base_color, 90 + 5*(root.level_counter)*(float)r.NextDouble(), 0.4f, state, root);
                //progressively increase lifetime as levels increase
            }

            //this actually gave me a pinwheel. not what I was going for but it sure looks cool. I'm keeping it.

            fireevent = false;
        }

        public void seeker(GameRoot root, GameTime gameTime)
        {
            //what must be done here

            //The seeker's color is not known until the player shoots it. That could be annoying.
            //So let's give him an exhaust trail matching his color.
            //And update him so that he only kills the player if their colors match.

            Vector2 player_position = EntityManager.ships[0].position;
            Vector2 direction = player_position - position;
            Vector2 direction_normalized = player_position - position;
            direction_normalized.Normalize();

            accel.X = (float)Math.Cos(angle) * 0.05f;
            accel.Y = (float)Math.Sin(angle) * 0.05f;

            //position += direction_normalized;
            //So what we could do is just use the above line to make the enemy track the player perfectly. But that's not very interesting.
            //Instead, we have the seeker check its angle vs the angle of the vector pointing at the player.
            //If they're off, adjust slightly.
            //This slight adjustment makes the seeker "overshoot" the player consistently if we're not just sitting still.
            //It feels more "alive" that way, if that makes sense.

            if (angle >= (float)Math.Atan2(direction.Y, direction.X))
            {
                rotation_speed -= 0.01f;
            }

            else if (angle < (float)Math.Atan2(direction.Y, direction.X))
            {
                rotation_speed += 0.01f;
            }



            velocity += accel;
            velocity *= friction;

            position += velocity;
            rotation_speed *= rotation_friction;
            angle += rotation_speed;

            //angle = (float)Math.Atan2(direction.Y, direction.X);//+ MathHelper.PiOver2;
            last_position = position;
        }
    }
}
