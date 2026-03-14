using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Design;
using Microsoft.Xna.Framework.Graphics;
using ParticleEngine;

namespace Spaceship
{
    //I don't know how to handle loot exactly.
    //It seems wasteful to iterate through the entire list and look for pickups that change the player's PW.
    //I could create a weapon class; when the "Trishot" loot is picked up, have the EM create a new instance
    //of the weapon class, and add it to the player.
    //Within the weapon class, include a weapontype enum, a PM field, and an initial PState field
    //In the constructor, create the PM, create the initial PState, and choose a preset ParticleState from the ParticleState struct for updates beyond initial.
    //Do all of that based on the enum on creation, and you're good.
    public class Weapon
    {
        public Texture2D bullet_texture;
        public ParticleManager<ParticleState> particleManager;
        public ParticleState initial_state;
        public GameRoot gameRoot;
        public WeaponType weaponType;
        public Ship player_ship;

        public enum WeaponType
        {
            Default,
            Trishot,
            Ringshot,
        }

        public Weapon(GameRoot root, Ship player, WeaponType type)
        {
            gameRoot = root;
            player_ship = player;
            weaponType = type;

            switch (weaponType)
            {
                case WeaponType.Default:                  
                    particleManager = new ParticleManager<ParticleState>(200, ParticleState.Update_PW_Bullets, ParticleManager<ParticleState>.ParticleAttribute.PlayerBullet, root);
                    break;
                case WeaponType.Trishot:
                    particleManager = new ParticleManager<ParticleState>(200, ParticleState.Update_PW_Bullets, ParticleManager<ParticleState>.ParticleAttribute.PlayerBullet, root);
                    break;
                case WeaponType.Ringshot:
                    particleManager = new ParticleManager<ParticleState>(200, ParticleState.Update_PW_Bullets, ParticleManager<ParticleState>.ParticleAttribute.PlayerBullet, root);
                    break;
                default:
                    break;
            }
        }

        ParticleState SelectInitialState(WeaponType type)
        {
            ParticleState initial_state = new ParticleState();
            switch (type)
            {
                case WeaponType.Default:
                    initial_state = new ParticleState()
                    {
                        ParentAngle = player_ship.angle,
                        AngleVector = player_ship.angle_vector * 5 + player_ship.velocity,
                        Type = ParticleType.None,
                        LengthMultiplier = 1f
                    };
                    break;
                case WeaponType.Trishot:
                    initial_state = new ParticleState()
                    {
                        ParentAngle = player_ship.angle,
                        AngleVector = player_ship.angle_vector * 5 + player_ship.velocity,
                        Type = ParticleType.None,
                        LengthMultiplier = 1f
                    };
                    break;
                case WeaponType.RingShot:
                    initial_state = new ParticleState()
                    {
                        ParentAngle = player_ship.angle,
                        AngleVector = player_ship.angle_vector * 5 + player_ship.velocity,
                        Type = ParticleType.None,
                        LengthMultiplier = 1f
                    }
                    break;
                default:
                    break;
            }
            return initial_state;
        }

        public void Fire(float duration, float scale, WeaponType type)
        {
            bullet_texture = gameRoot.circle;
            var initial_state = SelectInitialState(type);

            switch (type)
            {
                case WeaponType.Default:
                    particleManager.CreateParticle(bullet_texture, player_ship.position - gameRoot.player_origin, player_ship.color, player_ship.color, duration, scale, initial_state, gameRoot);
                    break;
                case WeaponType.Trishot:
                    for (int i = 0; i < 3; i++)
                    {
                        initial_state.AngleVector = player_ship.angle_vector * 5 + player_ship.velocity + new Vector2((float)Math.Sin(i*MathHelper.PiOver2),(float)Math.Cos(i*MathHelper.PiOver2));
                        particleManager.CreateParticle(bullet_texture, player_ship.position - gameRoot.player_origin, player_ship.color, player_ship.color, duration, scale, initial_state, gameRoot);
                    }
                    break;
                case WeaponType.Ringshot:
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = i * (MathHelper.TwoPi / 12);
                        initial_state.AngleVector = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle)) + player_ship.velocity;
                        particleManager.CreateParticle(bullet_texture, player_ship.position - gameRoot.player_origin, player_ship.color, player_ship.color, duration, scale, initial_state, gameRoot);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
