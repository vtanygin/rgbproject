//---------------------------------------------------------------------------------
// Written by Michael Hoffman
// Find the full tutorial at: http://gamedev.tutsplus.com/series/vector-shooter-xna/
//----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using Spaceship;
using System.Runtime.Serialization.Formatters;

namespace ParticleEngine
{
	public class ParticleManager<T>
	{
		// This delegate will be called for each particle.
		private Action<Particle> updateParticle;
		public CircularParticleArray particleList; //open up access to this array so we can use the list for collision detection
		public ParticleAttribute particleAttribute;
		public GameRoot root;
		public bool CollisionDetected = false;
		public Vector2 CollisionLocation = Vector2.Zero;

		//Note that nesting these doesn't work. IDK enough about generics to understand why.

		/// <summary>
		/// Determines collision behavior for the particles this manager generates.
		/// </summary>
		public enum ParticleAttribute
		{
			Inert,
			PlayerBullet,
			EnemyBullet
		};

		/// <summary>
		/// Allows creation of particles.
		/// </summary>
		/// <param name="capacity">The maximum number of particles. An array of this size will be pre-allocated.</param>
		/// <param name="updateParticle">A delegate that lets you specify custom behaviour for your particles. Called once per particle, per frame.</param>
		/// <param name="attribute">Determines certain preset custom behavior.</param>
		public ParticleManager(int capacity, Action<Particle> updateParticle, ParticleAttribute attribute, GameRoot gameRoot)
		{
			this.updateParticle = updateParticle;
			root = gameRoot;
			particleList = new CircularParticleArray(capacity);

			// Populate the list with empty particle objects, for reuse.
			for (int i = 0; i < capacity; i++)
				particleList[i] = new Particle();
			particleAttribute = attribute;
		}

		/// <summary>
		/// Update particle state, to be called every frame.
		/// </summary>
		public void Update()
		{
			int removalCount = 0;
			for (int i = 0; i < particleList.Count; i++)
			{
				var particle = particleList[i];
				updateParticle(particle); //whatever static method we defined in ParticleState and supplied as a param for this PM, gets called here.
				particle.PercentLife -= 1f / particle.Duration;

				// sift deleted particles to the end of the list
				Swap(particleList, i - removalCount, i);

				// if the particle has expired, delete this particle
				if (particle.PercentLife < 0)
					removalCount++;
			}
			particleList.Count -= removalCount;
			HandleCollisions();
		}

		/// <summary>
		/// Each PM reads from the EntityManager's master list of entities to handle collisions.
		/// If you want particles that interact with **other particles,** then you should create a dedicated class for them that inherits from Entity.
		/// </summary>
		public void HandleCollisions()
		{
			if (particleAttribute == ParticleAttribute.Inert)
			{
				return; //Inert particles are just for show, so we don't want to waste cycles on collsion checks on them.
			}
			
			var enemies = EntityManager.enemies;
			var ships = EntityManager.ships;

			if (particleAttribute == ParticleAttribute.EnemyBullet)
			{

				for (int i = 0; i < ships.Count; i++)
				{
					for (int j = 0; j < ParticleCount; j++)
					{
						//root.testvar = particleList[j].Position.X;
						//root.testvar = Vector2.Distance(ships[i].position - new Vector2(ships[i].texture.Width/2,ships[i].texture.Height/2), particleList[j].Position);
						//What a crock of shit. IDK why simply copying the condition from the player bullet code didn't work.
						//Took me way too long to find a workaround. It's still kinda sloppy but better than what copying the playerbullet code did (aka nothing)

						bool colors_match = false;

						if (ships[i].color == particleList[j].BaseColor)
						{
							colors_match = true;
						}

						if ((Vector2.Distance(ships[i].position - new Vector2(ships[i].scale*ships[i].texture.Width/2, ships[i].scale * ships[i].texture.Height/2), particleList[j].Position ) < particleList[j].Texture.Width*2) && !ships[i].IsDead && colors_match)
						{

							//particleList[j].root.testvar += 0.1f;
							ships[i].health -= 10f*particleList[j].PercentLife;
							particleList[j].PercentLife = 0; //delete bullet on contact
							CollisionDetected = true;
							CollisionLocation = particleList[j].Position;

							//enemies[i].body_color.B -= (byte)1; 
							//now here's a neat way to visually indicate health. "Show, don't tell" and all that.
							//But it's best to give objects maximum control over their own properties, so we let the Enemy class set its color based on its HP, rather than having the PM do that.

							//do stuff
						}
					}
				}
			}

			if (particleAttribute == ParticleAttribute.PlayerBullet)
			{
				// handle collisions between bullets and enemies
				for (int i = 0; i < enemies.Count; i++)
					for (int j = 0; j < ParticleCount; j++)
					{
						bool colors_match = false;
						if (enemies[i].base_color == particleList[j].BaseColor)
						{
							colors_match = true;
						}
						//root.testvar = Vector2.Distance(enemies[i].position, particleList[j].Position - new Vector2(enemies[i].texture.Width / 2f));
						//we can't access the "particle type" enum for particles, because they **reference** it via the ParticleState struct and don't actually own it.
						//That's annoying, so we'll make a new enum and put it directly as a property of the PM that generates the particles.
						//That way we can call this method but only do the collision checks if the particles are "hot" (i.e. meant to interact with stuff).
						//Going to leave the old one in place for now because I can't be bothered to clean up the references atm.

						if ((Vector2.Distance(enemies[i].position, particleList[j].Position - new Vector2(enemies[i].texture.Width / 2f)) < particleList[j].Texture.Width) && !enemies[i].IsDead && colors_match)
						{
							//particleList[j].root.testvar += 0.1f;
							enemies[i].health -= 25f;
							particleList[j].PercentLife = 0; //delete bullet on contact
							CollisionDetected = true;
							CollisionLocation = particleList[j].Position;

							//enemies[i].body_color.B -= (byte)1; 
							//now here's a neat way to visually indicate health. "Show, don't tell" and all that.
							//But it's best to give objects maximum control over their own properties, so we let the Enemy class set its color based on its HP, rather than having the PM do that.

							//do stuff
						}
					}
			}

			
		}

		private static void Swap(CircularParticleArray list, int index1, int index2)
		{
			var temp = list[index1];
			list[index1] = list[index2];
			list[index2] = temp;
		}

		/// <summary>
		/// Draw the particles.
		/// </summary>
		public void Draw(SpriteBatch spriteBatch)
		{
			for (int i = 0; i < particleList.Count; i++)
			{
				var particle = particleList[i];

				Vector2 origin = new Vector2(particle.Texture.Width / 2, particle.Texture.Height / 2);
				spriteBatch.Draw(particle.Texture, particle.Position, null, particle.Color, particle.Orientation, origin, particle.Scale, 0, 0);
			}
		}

		public void CreateParticle(Texture2D texture, Vector2 position, Color tint, float duration, float scale, T state, GameRoot gameRoot, float theta = 0)
		{
			CreateParticle(texture, position, tint, duration, new Vector2(scale), state, gameRoot, theta);
		}

		public void CreateParticle(Texture2D texture, Vector2 position, Color tint, Color base_color, float duration, float scale, T state, GameRoot gameRoot, float theta = 0)
		{
			CreateParticle(texture, position, tint, base_color, duration, new Vector2(scale), state, gameRoot, theta);
		}

		public void CreateParticle(Texture2D texture, Vector2 position, Color tint, float duration, Vector2 scale, T state, GameRoot gameRoot, float theta = 0)
		{
			Particle particle;
			if (particleList.Count == particleList.Capacity)
			{
				// if the list is full, overwrite the oldest particle, and rotate the circular list
				particle = particleList[0];
				particleList.Start++;
			}
			else
			{
				particle = particleList[particleList.Count];
				particleList.Count++;
			}

			// Create the particle
			particle.Texture = texture;
			particle.Position = position;
			particle.Color = tint;

			particle.Duration = duration;
			particle.PercentLife = 1f;
			particle.Scale = scale;
			particle.Orientation = theta;
			particle.State = state;
			particle.root = gameRoot;
		}

		public void CreateParticle(Texture2D texture, Vector2 position, Color tint, Color base_color, float duration, Vector2 scale, T state, GameRoot gameRoot, float theta = 0)
		{
			Particle particle;
			if (particleList.Count == particleList.Capacity)
			{
				// if the list is full, overwrite the oldest particle, and rotate the circular list
				particle = particleList[0];
				particleList.Start++;
			}
			else
			{
				particle = particleList[particleList.Count];
				particleList.Count++;
			}

			// Create the particle
			particle.Texture = texture;
			particle.Position = position;
			particle.Color = tint;
			particle.BaseColor = base_color; //this is the color before we apply any effects, and is used for collision detection color matching

			particle.Duration = duration;
			particle.PercentLife = 1f;
			particle.Scale = scale;
			particle.Orientation = theta;
			particle.State = state;
			particle.root = gameRoot;
		}

		/// <summary>
		/// Destroys all particles
		/// </summary>
		public void Clear()
		{
			particleList.Count = 0;
		}

		public int ParticleCount
		{
			get { return particleList.Count; }
		}

		public class Particle
		{
			public Texture2D Texture;
			public Vector2 Position;
			public float Orientation;

			public Vector2 Scale = Vector2.One;

			public Color Color;
			public Color BaseColor;
			public float Duration;
			public float PercentLife = 1f;
			public T State;

			public GameRoot root;
		}

		// Represents a circular array with an arbitrary starting point. It's useful for efficiently overwriting
		// the oldest particles when the array gets full. Simply overwrite particleList[0] and advance Start.
		public class CircularParticleArray
		{
			private int start;
			public int Start
			{
				get { return start; }
				set { start = value % list.Length; }
			}

			public int Count { get; set; }
			public int Capacity { get { return list.Length; } }
			public Particle[] list;

			public CircularParticleArray() { }  // for serialization

			public CircularParticleArray(int capacity)
			{
				list = new Particle[capacity];
			}

			public Particle this[int i]
			{
				get { return list[(start + i) % list.Length]; }
				set { list[(start + i) % list.Length] = value; }
			}
		}
	}
}
