using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Penumbra;
using ParticleEngine;
using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Spaceship
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameRoot : Game
    {
        public GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public float testvar;

        public Texture2D ship;
        public Texture2D asteroid;
        public Texture2D space;
        public Texture2D triangle;
        public Texture2D circle;
        public Texture2D kirby;
        public Texture2D kirby_white;
        public Texture2D triangle_right;

        public Color[] colors;
        public int color_index;
        public int level_counter;

        Ship player;
        Enemy triangle_enemy;

        SpriteFont gamefont;
        SpriteFont timerfont;

        public Vector2 player_origin = new Vector2(58 / 2f, 60 / 2f); //height and width of the sprite since nothing outside LoadContent can see this otherwise
        public Vector2 triangle_origin = new Vector2(32 / 2f);

        void WallCollisionCheck()
        {
            if (player.position.X >= graphics.PreferredBackBufferWidth || player.position.X <= player_origin.X)
            {
                player.velocity.X *= -1;
            }

            if (player.position.Y >= graphics.PreferredBackBufferHeight || player.position.Y <= player_origin.Y)
            {
                player.velocity.Y *= -1;
            }

            foreach (var enemy in EntityManager.enemies)
            {
                if (enemy.enemyType == Enemy.EnemyType.Seeker)
                {
                    if (enemy.position.X >= graphics.PreferredBackBufferWidth || enemy.position.X <= enemy.texture.Width)
                    {
                        enemy.velocity.X *= -1;
                    }

                    if (enemy.position.Y >= graphics.PreferredBackBufferHeight || enemy.position.Y <= enemy.texture.Height)
                    {
                        enemy.velocity.Y *= -1;
                    }
                }
            }
        }
        public GameRoot()
        {
            //PenumbraComponent penumbra = new PenumbraComponent(this);
            //Components.Add(penumbra);
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1377;
            graphics.PreferredBackBufferHeight = 720;

            IsMouseVisible = true;
            player = new Ship(this);
            triangle_enemy = new Enemy(this, Enemy.EnemyType.Seeker);
            triangle_enemy.base_color = Color.Blue;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            EntityManager.Add(player);
            player.scale = 1f;
            EntityManager.Add(triangle_enemy);

            colors = new Color[3];
            colors[0] = Color.Blue;
            colors[1] = Color.Green;
            colors[2] = Color.Red;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ship = Content.Load<Texture2D>("ship");


            asteroid = Content.Load<Texture2D>("asteroid");
            space = Content.Load<Texture2D>("space");

            triangle = Content.Load<Texture2D>("triangle");
            triangle_right = Content.Load<Texture2D>("triangle_right");
            triangle_enemy.texture = triangle_right;

            circle = Content.Load<Texture2D>("circle");
            kirby = Content.Load<Texture2D>("kirby");
            kirby_white = Content.Load<Texture2D>("kirby_white");
            player.texture = kirby_white;

            Song default_song = Content.Load<Song>("default_song");


            gamefont = Content.Load<SpriteFont>("spaceFont");
            timerfont = Content.Load<SpriteFont>("timerfont");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //player.Update(this, gameTime);
            //triangle_enemy.Update(this, gameTime);

            WallCollisionCheck();

            EntityManager.Update(this,gameTime); //All player and enemy update behavior offloaded here now.

            if (EntityManager.enemies.Count == 0)
            {
                level_counter++;
                LevelManager.GenerateRandomLevel(this);

            }


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(space, Vector2.Zero, Color.White);

            foreach (var enemy in EntityManager.enemies)
            {
                if (!enemy.IsDead)
                {
                    spriteBatch.Draw(enemy.texture, enemy.position + new Vector2(16), null, enemy.body_color, enemy.angle, triangle_origin, 1f, SpriteEffects.None, 0f);
                }
            }

            foreach (var ship in EntityManager.ships)
            {
                if (!ship.IsDead)
                {
                    spriteBatch.Draw(ship.texture, new Vector2(player.position.X - 34, player.position.Y - 50), null, ship.color, ship.angle, player_origin, player.scale, SpriteEffects.None, 0f);
                }
            }

            // if (!triangle_enemy.IsDead)
            // {
            //    spriteBatch.Draw(triangle, triangle_enemy.position + new Vector2(16), null, triangle_enemy.body_color, triangle_enemy.angle, triangle_origin, 1f, SpriteEffects.None, 0f);
            // }

            //spriteBatch.Draw(player.texture, new Vector2(player.position.X - 34, player.position.Y - 50), null, Color.White, player.angle, player_origin, 1f, SpriteEffects.None, 0f);
            //For now the drawing of the actual entities is still here. Offload to EntityManager later.


            //spriteBatch.DrawString(timerfont, player.angle_vector.ToString(),new Vector2(150),Color.White);
            spriteBatch.DrawString(timerfont, "Health: " + testvar.ToString(), new Vector2(50), Color.White);
            spriteBatch.DrawString(timerfont, "Level: " + level_counter.ToString(), new Vector2(1200,50), Color.White);

            EntityManager.Draw(this, spriteBatch); //This calls the methods that draw anything else that the player and enemies create (particles mostly)

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
