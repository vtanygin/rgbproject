//---------------------------------------------------------------------------------
// Written by Michael Hoffman
// Find the full tutorial at: http://gamedev.tutsplus.com/series/vector-shooter-xna/
//----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Design;
using Spaceship;

namespace ParticleEngine
{

	public enum ParticleType { None, Enemy, Bullet, IgnoreGravity }

	public struct ParticleState
	{
		public Vector2 AngleVector;
		public ParticleType Type;
		public float LengthMultiplier;
		public float ParentAngle;

		private static Random rand = new Random();
		static float NextFloat(Random random, float min, float max)
		{
			var buffer = new byte[4];
			random.NextBytes(buffer);
			float output = BitConverter.ToSingle(buffer, 0);

			if (output < min)
			{
				return min;
			}
			else if (output > max)
			{
				return max;
			}
			else
			{
				return output;
			}
		}

		/*public ParticleState(Vector2 velocity, ParticleType type, float lengthMultiplier = 1f)
		{
			Velocity = velocity;
			Type = type;
			LengthMultiplier = lengthMultiplier;
		}*/

		public ParticleState(float parentangle, Vector2 anglevector, ParticleType type, float lengthMultiplier = 1f)
		{

			AngleVector = anglevector;
			Type = type;
			LengthMultiplier = lengthMultiplier;
			ParentAngle = parentangle;
		}

		public static ParticleState GetRandom(float minVel, float maxVel)
		{
			var state = new ParticleState();
			state.AngleVector.X = NextFloat(rand, minVel, maxVel);
			state.AngleVector.Y = NextFloat(rand, minVel, maxVel);
			state.Type = ParticleType.None;
			state.LengthMultiplier = 1;

			return state;
		}

		public static void Update_PW_Bullets(ParticleManager<ParticleState>.Particle particle)
		{
			//var vel_x = particle.State.AngleVector.X;
			//var vel_y = particle.State.AngleVector.Y;
			var vel = particle.State.AngleVector;
			float speed = vel.Length();

			// using Vector2.Add() should be slightly faster than doing "x.Position += vel;" because the Vector2s
			// are passed by reference and don't need to be copied. Since we may have to update a very large 
			// number of particles, this method is a good candidate for optimizations.

			//Vector2.Add(ref particle.Position, ref vel, out particle.Position);

			// fade the particle if its PercentLife or speed is low.
			float alpha = Math.Min(1, Math.Min(particle.PercentLife * 2, speed * 1f));
			alpha *= alpha;

			particle.Color.A = (byte)(255 * alpha);

			// the length of bullet particles will be less dependent on their speed than other particles
			if (particle.State.Type == ParticleType.Bullet)
				particle.Scale.X = particle.State.LengthMultiplier * Math.Min(Math.Min(1f, 0.1f * speed + 0.1f), alpha);
			else
				//particle.Scale.X = particle.State.LengthMultiplier * Math.Min(Math.Min(1f, 0.2f * rand.Next(-5,5) + 0.1f), alpha);
				//particle.Scale.Y = particle.State.LengthMultiplier * Math.Min(Math.Min(1f, 0.2f * rand.Next(-5,5) + 0.1f), alpha);

				particle.Orientation = particle.State.ParentAngle - MathHelper.PiOver2;//(float)Math.Asin(vel.X); //+ MathHelper.PiOver2;

			ParticleEffects.WallCollision(particle);

			//particle.Color = Color.Lerp(particle.Color, Color.TransparentBlack, 0.01f);


			if (particle.State.Type != ParticleType.IgnoreGravity)
			{

			}

			/*if (Math.Abs(vel.X) + Math.Abs(vel.Y) < 0.00000000001f)	// denormalized floats cause significant performance issues
				vel = Vector2.Zero;
			else if (particle.State.Type == ParticleType.Enemy)
				vel *= 0.94f;
			else
			{
				vel *= 0.96f + Math.Abs(pos.X) % 0.04f; // rand.Next() isn't thread-safe, so use the position for pseudo-randomness
			}*/

			particle.Position += vel;

			//particle.State.AngleVector = vel; this line runs everything so don't uncomment
		}

		public static void UpdateParticle(ParticleManager<ParticleState>.Particle particle)
		{
			//var vel_x = particle.State.AngleVector.X;
			//var vel_y = particle.State.AngleVector.Y;
			var vel = particle.State.AngleVector;
			float speed = vel.Length();

			// using Vector2.Add() should be slightly faster than doing "x.Position += vel;" because the Vector2s
			// are passed by reference and don't need to be copied. Since we may have to update a very large 
			// number of particles, this method is a good candidate for optimizations.

			//Vector2.Add(ref particle.Position, ref vel, out particle.Position);

			// fade the particle if its PercentLife or speed is low.
			float alpha = Math.Min(1, Math.Min(particle.PercentLife * 2, speed * 1f));
			alpha *= alpha;

			particle.Color.A = (byte)(255 * alpha);

			// the length of bullet particles will be less dependent on their speed than other particles
			if (particle.State.Type == ParticleType.Bullet)
				particle.Scale.X = particle.State.LengthMultiplier * Math.Min(Math.Min(1f, 0.1f * speed + 0.1f), alpha);
			else
				//particle.Scale.X = particle.State.LengthMultiplier * Math.Min(Math.Min(1f, 0.2f * rand.Next(-5,5) + 0.1f), alpha);
				//particle.Scale.Y = particle.State.LengthMultiplier * Math.Min(Math.Min(1f, 0.2f * rand.Next(-5,5) + 0.1f), alpha);

			particle.Orientation = particle.State.ParentAngle - MathHelper.PiOver2;//(float)Math.Asin(vel.X); //+ MathHelper.PiOver2;

			ParticleEffects.WallCollision(particle);

			particle.Color = Color.Lerp(particle.Color, Color.TransparentBlack, 0.01f);

			//Color color = Color.Lerp(Color.Blue, Color.White, (float)rand.NextDouble());


			if (particle.State.Type != ParticleType.IgnoreGravity)
			{
			
			}

			/*if (Math.Abs(vel.X) + Math.Abs(vel.Y) < 0.00000000001f)	// denormalized floats cause significant performance issues
				vel = Vector2.Zero;
			else if (particle.State.Type == ParticleType.Enemy)
				vel *= 0.94f;
			else
			{
				vel *= 0.96f + Math.Abs(pos.X) % 0.04f; // rand.Next() isn't thread-safe, so use the position for pseudo-randomness
			}*/

			particle.Position += vel;

			//particle.State.AngleVector = vel; this line runs everything so don't uncomment
		}

		public static void EnemyDeathExplosion(ParticleManager<ParticleState>.Particle particle)
		{

		}

		public static void PW_Splash(ParticleManager<ParticleState>.Particle particle)
		{
			//cosmetic splash effects when player bullets damage an enemy

			//var vel_x = particle.State.AngleVector.X;
			//var vel_y = particle.State.AngleVector.Y;
			var vel = particle.State.AngleVector;
			float speed = vel.Length();

			// using Vector2.Add() should be slightly faster than doing "x.Position += vel;" because the Vector2s
			// are passed by reference and don't need to be copied. Since we may have to update a very large 
			// number of particles, this method is a good candidate for optimizations.

			//Vector2.Add(ref particle.Position, ref vel, out particle.Position);

			// fade the particle if its PercentLife or speed is low.
			float alpha = Math.Min(1, particle.PercentLife);
			alpha *= alpha;

			particle.Color.A = (byte)(255 * particle.PercentLife);

			particle.Scale.X = particle.State.LengthMultiplier * Math.Min(Math.Min(1f, 0.2f * rand.Next(-1,1) + 0.1f), alpha);
			particle.Scale.Y = particle.State.LengthMultiplier * Math.Min(Math.Min(1f, 0.2f * rand.Next(-1,1) + 0.1f), alpha);

				particle.Orientation = particle.State.ParentAngle - MathHelper.PiOver2;//(float)Math.Asin(vel.X); //+ MathHelper.PiOver2;

			particle.Color = Color.Lerp(particle.Color, Color.TransparentBlack, 0.01f);


			if (particle.State.Type != ParticleType.IgnoreGravity)
			{

			}

			/*if (Math.Abs(vel.X) + Math.Abs(vel.Y) < 0.00000000001f)	// denormalized floats cause significant performance issues
				vel = Vector2.Zero;
			else if (particle.State.Type == ParticleType.Enemy)
				vel *= 0.94f;
			else
			{
				vel *= 0.96f + Math.Abs(pos.X) % 0.04f; // rand.Next() isn't thread-safe, so use the position for pseudo-randomness
			}*/

			particle.Position += vel;
		}
	}

	/// <summary>
	/// a collection of methods for applying different visual effects (such as color changes) and behavior (such as wall collision) to particles.
	/// </summary>
	public static class ParticleEffects
	{
		/// <summary>
		/// Call this method to make particles have elastic collisions with the edges of the game window.
		/// </summary>
		/// <param name="particle"></param>
		public static void WallCollision(ParticleManager<ParticleState>.Particle particle)
		{
			var pos = particle.Position;
			int width = (int)particle.root.graphics.PreferredBackBufferWidth;
			int height = (int)particle.root.graphics.PreferredBackBufferHeight;
			// collide with the edges of the screen
			if (pos.X < 0 + particle.Texture.Width)
			{
				particle.State.AngleVector.X = Math.Abs(particle.State.AngleVector.X);
				particle.Position.X = particle.Texture.Width;
			}

			else if (pos.X > width - particle.Texture.Width)
			{
				particle.State.AngleVector.X = -Math.Abs(particle.State.AngleVector.X);
				particle.Position.X = width - particle.Texture.Width;
			}

			if (pos.Y < 0 + particle.Texture.Height)
			{
				particle.State.AngleVector.Y = Math.Abs(particle.State.AngleVector.Y);
				particle.Position.Y = particle.Texture.Height;
			}

			else if (pos.Y > height - particle.Texture.Height)
			{
				particle.State.AngleVector.Y = -Math.Abs(particle.State.AngleVector.Y);
				particle.Position.Y = height - particle.Texture.Height;
			}
		}
	}
}
