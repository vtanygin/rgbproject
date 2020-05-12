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
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace Spaceship
{
    public class Ship : Entity
    {
        public override float scale { get; set; }

        public override Vector2 origin { get; set; }
        public override bool DoDraw { get; set; }

        public SoundEffectInstance gun_loop;
        public SoundEffectInstance enemy_hurt;
        public SoundEffectInstance life_up;

        private const float _delay = 2; // seconds
        private float _remainingDelay = _delay;
        private bool fireevent = false;

        private float _playergun_delay = 0.1f;
        private float _playergun_remainingdelay;
        private bool primary_fired = false;

        public Color[] colors;
        public Color color;
        public int color_index;
        public bool color_changed = false;

        public int lives;
        private bool life_score_bonus_given = false;
        private bool life_decremented = false;

        public float health = 100;
        public override Texture2D texture { get; set; }
        public bool IsDead = false;
        public override Vector2 position { get; set; }
        public Vector2 velocity = Vector2.Zero;
        public Vector2 accel = Vector2.Zero;
        public float angle = 0f;
        public float friction = 1f;

        public Vector2 pos_normalized;
        public Vector2 angle_vector;

        public Weapon primary_weapon;

        public List<Loot> pickups;

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
            DoDraw = true;
            position = new Vector2(100);//new Vector2(rand.Next(root.graphics.PreferredBackBufferWidth), rand.Next(root.graphics.PreferredBackBufferHeight));
            texture = root.kirby_white;
            lives = 1;
            scale = 1f;

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

            pickups = new List<Loot>();
            primary_weapon = new Weapon(root, this, Weapon.WeaponType.Default);

            //Now look at this. UpdateParticle came with the engine, but now we made a custom update method for the main gun's bullets.
        }

       /* Ship(GameRoot root, int dummy)
        {
            texture = root.kirby_white;
            IsDead = false;
        }

        public Ship RespawnShip(GameRoot root, Ship oldship)
        {
            Ship newship = new Ship(root, 0);
            newship.deathExplosion = oldship.deathExplosion;
            newship.primaryweapon_PM = oldship.primaryweapon_PM;
            newship.PW_Splash = oldship.PW_Splash;
            newship.position = oldship.position;
            newship.color = oldship.

            return newship;
        }*/

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
            //for ease of reading, pull out all of these condition checks into separate methods with clear names giving their purpose, and then just call all the methods inside Update()
            if (root.player_gun_sfx!= null && gun_loop == null)
            {
                gun_loop = root.player_gun_sfx.CreateInstance();
               // gun_loop.IsLooped = true;
            }

            if (root.enemy_hurt_sfx != null && enemy_hurt == null)
            {
                enemy_hurt = root.enemy_hurt_sfx.CreateInstance();
            }

            if (root.life_up_sfx != null && life_up == null)
            {
                life_up = root.life_up_sfx.CreateInstance();
            }

            pos_normalized = position;
            pos_normalized.Normalize();

            var x = (float)Math.Cos(angle);
            var y = (float)Math.Sin(angle);

            angle_vector = new Vector2(x,y);

            if (root.score % 500 == 0 && !life_score_bonus_given && root.score!=0)
            {
                lives++;
                life_up.Play();
                life_score_bonus_given = true;
            }

            else if (root.score % 500 !=0)
            {
                life_score_bonus_given = false;
            }

            if (keyboardState.IsKeyDown(Keys.A))
            {
                gun_loop.Play();
            }

            else
            {
                gun_loop.Stop();
            }


            if (keyboardState.IsKeyDown(Keys.A) && !primary_fired)
            {
                var timer = (float)gameTime.ElapsedGameTime.TotalSeconds;

                _playergun_remainingdelay -= timer;

                if (_playergun_remainingdelay <= 0)
                {
                    primary_weapon.Fire(300, 0.5f, primary_weapon.weaponType);
                    //PrimaryWeapon(root);
                    primary_fired = true;
                    _playergun_remainingdelay = _playergun_delay;
                }
                
            }

            primary_fired = false;

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
                    var respawn_pos = position;

                    if (lives > 0  && !life_decremented)
                    {
                        lives--;
                        life_decremented = true;
                        health = 100;
                        velocity = Vector2.Zero;
                        accel = Vector2.Zero;
                        //EntityManager.Add(this); //If we still have extra lives, I found it was simpler to persist the ship in the EntityManager on death...
                        //...and just put Kirby back where he died with zero vel/accel, and turn off the IsDead flag so the game resumes drawing and updating him.
                        IsDead = false;
                    }
                }
            }

            life_decremented = false;

            Movement();

            if (primary_weapon.particleManager.CollisionDetected) //if the main gun registers a collision, tell the splashy PM to make the splash effects. 
            {
                enemy_hurt.Play();
                for (int i = 0; i < 10; i++)
                {
                    var state = new ParticleState()
                    {
                        //ParentAngle = (float)rand.NextDouble();
                        AngleVector = new Vector2((float)rand.NextDouble() * rand.Next(-3, 3), (float)rand.NextDouble() * rand.Next(-3, 3)),
                        Type = ParticleType.None,
                        LengthMultiplier = 1f
                    };
                    PW_Splash.CreateParticle(root.kirby, primary_weapon.particleManager.CollisionLocation, Color.White, 150, 0.1f, state, root);
                }

                root.score += 25;
                primary_weapon.particleManager.CollisionDetected = false;
                //primaryweapon_PM.CollisionDetected = false; //turn off the fireworks after we've finished detonating
            }

            root.testvar = health;
            primary_weapon.particleManager.Update(); //could wrap the PM Update() call in a Update() for the weapon itself
            //primaryweapon_PM.Update();
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
                AngleVector = angle_vector * 5+ velocity,
                Type = ParticleType.None,
                LengthMultiplier = 1f
            };

            if (false)
            {
                //I don't know how to handle loot exactly.
                //It seems wasteful to iterate through the entire list and look for pickups that change the player's PW.
                //I could create a weapon class; when the "Trishot" loot is picked up, have the EM create a new instance
                //of the weapon class, and add it to the player.
                //Within the weapon class, include a weapontype enum, a PM field, and an initial PState field
                //In the constructor, create the PM, create the initial PState, and choose a preset ParticleState from the ParticleState struct for updates beyond initial.
                //Do all of that based on the enum on creation, and you're good.
            }

            primaryweapon_PM.CreateParticle(root.circle, position - root.player_origin , color, color, 300, 0.5f, state, root); //reassign origin to a ship property later
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

        public override void Draw(GameRoot root, SpriteBatch batch)
        {
            //primaryweapon_PM.Draw(batch);
            primary_weapon.particleManager.Draw(batch); //could wrap the PM Draw() call in a Draw() for the weapon itself
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
