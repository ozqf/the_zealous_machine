using Godot;
using TheZealousMachine.actors.projectiles;

namespace TheZealousMachine
{
    public interface IGame
    {
        void RegisterPlayer(IPlayer player);
        void UnregisterPlayer(IPlayer player);
        TargetInfo GetPlayerTarget();
        ProjectileGeneric CreateProjectile();
        public void AddMouseLock(string lockName);
        public void RemoveMouseLock(string lockName);
        public bool IsMouseLocked();
    }

    public struct HitInfo
    {
        public int damage;
        public Vector3 position;
        public Vector3 direction;
    }

    public enum HitResposneType
    {
        Damaged, Immune, Ignored
    }

    public struct HitResponse
    {
        public static HitResponse Empty
        {
            get
            {
                return new HitResponse
                {
                    type = HitResposneType.Ignored,
                    damage = 0
                };
            }
        }

        public HitResposneType type;
        public int damage;
    }

    public interface IHittable
    {
        public HitResponse Hit(HitInfo hit);
    }

    public interface IArena
    {
        public void TriggerTouched(string name, string message);
    }

    public interface IVolume
    {
        public void SetOn(bool flag);
    }

    public interface IPlayer
    {
        TargetInfo GetTargetInfo();
    }

    public struct TargetInfo
    {
        public bool valid;
        public Vector3 position;
    }

    public struct ProjectileLaunchInfo
    {
        public Vector3 position;
        public Vector3 forward;
        public float speed;
        public int teamId;
        public int damage;
    }
    public static class Interactions
    {
        public const uint BIT_WORLD = (1 << 0);
        public const uint BIT_PLAYER = (1 << 1);
        public const uint BIT_MOBS = (1 << 2);
        public const uint BIT_PLAYER_PROJECTILE = (1 << 3);
        public const uint BIT_MOB_PROJECTILE = (1 << 4);
        public const uint BIT_TRIGGERS = (1 << 5);
        public const uint BIT_ITEMS = (1 << 6);
        public const uint BIT_PLAYER_BARRIER = (1 << 7);
        public const uint BIT_DEBRIS = (1 << 8);
        public static uint GetPlayerProjectileMask()
        {
            return (BIT_WORLD | BIT_MOBS);
        }
    }
}
