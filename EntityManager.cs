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
using ParticleEngine;

namespace Spaceship
{
	static class EntityManager
	{
		public static List<Entity> entities = new List<Entity>();
		public static List<Enemy> enemies = new List<Enemy>();
		public static List<Ship> ships = new List<Ship>();
		public static List<Loot> loots = new List<Loot>();

		static bool isUpdating;
		static List<Entity> addedEntities = new List<Entity>();

		public static int Count { get { return entities.Count; } }

		public static void Add(Entity entity)
		{
			if (!isUpdating)
				AddEntity(entity);
			else
				addedEntities.Add(entity);
		}

		private static void AddEntity(Entity entity)
		{
			entities.Add(entity);
			if (entity is Enemy)
				enemies.Add(entity as Enemy);
			if (entity is Ship)
				ships.Add(entity as Ship);
			if (entity is Loot)
				loots.Add(entity as Loot);
		}

		public static void Update(GameRoot gameRoot, GameTime gameTime)
		{
			isUpdating = true;
			HandleCollisions(gameRoot);

			foreach (var entity in entities)
				entity.Update(gameRoot, gameTime);

			isUpdating = false;

			foreach (var entity in addedEntities)
				AddEntity(entity);

			addedEntities.Clear();

			//entities = entities.Where(x => !x.IsExpired).ToList();
			//enemies = enemies.Where(x => !x.IsExpired).ToList();
		}

		static void HandleCollisions(GameRoot gameRoot)
		{
			//So for particles...I found it easier to have...
			//...each PM handle collision detection independently.
			//Is this bad/messy practice? IDK.

			// we have no inter-enemy collisions for now

			/*for (int i = 0; i < enemies.Count; i++)
				for (int j = i + 1; j < enemies.Count; j++)
				{
					if (IsColliding(enemies[i], enemies[j]))
					{
						enemies[i].HandleCollision(enemies[j]);
						enemies[j].HandleCollision(enemies[i]);
					}
				} */


			// bullets are treated as particles in my game, so the EM doesn't handle this.
			/*for (int i = 0; i < enemies.Count; i++)
				for (int j = 0; j < bullets.Count; j++)
				{
					if (IsColliding(enemies[i], bullets[j]))
					{
						enemies[i].WasShot();
						bullets[j].IsExpired = true;
					}
				}*/

			//If the seeker collides with a same-color player, they both die.
			for (int i = 0; i < enemies.Count; i++)
			{
				bool colors_match = false;
				if (enemies[i].base_color == ships[0].color)
				{
					colors_match = true;
				}
				if (enemies[i].enemyType == Enemy.EnemyType.Seeker && IsColliding(ships[0], enemies[i]) && !ships[0].IsDead && !enemies[i].IsDead && colors_match)
				{
					//ships[0].health = 0;
					//ships[0].Kill(gameRoot); //ships[0] is always the player ship fyi
					enemies[i].Kill(gameRoot); 
					break;
				}
			}

			//Loot pickups handled here
			for (int i = 0; i < loots.Count; i++)
			{
				if (IsColliding(ships[0], loots[i]) && !ships[0].IsDead)
				{
					bool pickthislootup = true; //by default we grab everything...
					foreach (var player_loot in ships[0].pickups)
					{
						if (Loot.sameLootType(player_loot,loots[i]) && !loots[i].stackable)
						{
							pickthislootup = false; //...but if we've found something the player already has, and doesn't stack, don't pick it up.
							break; //as soon as we find something non-stackable, no need to keep looping; break out and continue.
						}
					}

					if (pickthislootup)
					{
						loots[i].pickedup = true;
						ships[0].pickups.Add(loots[i]);
					}

				}
			}
		}

		private static bool IsColliding(Entity a, Entity b)
		{
			//yeah I should fix this as it's kinda busted
			float radius = a.texture.Width + b.texture.Width;
			return Vector2.DistanceSquared(a.position, b.position) < radius * radius;
		}

		/*public static IEnumerable<Entity> GetNearbyEntities(Vector2 position, float radius)
		{
			return entities.Where(x => Vector2.DistanceSquared(position, x.Position) < radius * radius);
		}*/

		public static void Draw(GameRoot gameRoot, SpriteBatch spriteBatch)
		{
			foreach (var entity in entities)
				entity.Draw(gameRoot, spriteBatch);
		}
	}
}