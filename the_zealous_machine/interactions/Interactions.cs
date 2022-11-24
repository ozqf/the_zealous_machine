using Godot;
using System;
using TheZealousMachine.actors.projectiles;
using TheZealousMachine.actors.volumes;

namespace TheZealousMachine
{
    public enum ImpactType
    {
        Yellow,
        Purple,
        Grey
    }

    public static class GameEvents
    {
        public const string MOB_DIED = "mob_died";
    }

    public interface IGame
    {
        // generic stuff
        void RegisterPlayer(IPlayer player);
        void UnregisterPlayer(IPlayer player);
        TargetInfo GetPlayerTarget();
        void AddMouseLock(string lockName);
        void RemoveMouseLock(string lockName);
        bool IsMouseLocked();

        int AssignActorId();

        // game specific
        Node3D CreateBulletImpact(Vector3 pos, Vector3 directon, ImpactType type = 0);
        ProjectileGeneric CreateProjectile(int type = 0);
        SpawnVolume CreateSpawnVolume(Vector3 pos);
        IMob CreateMob(Vector3 pos, int type = 0);
        Node3D CreateMobDebris(Vector3 pos, Vector3 direction);
    }

    public interface IMob
    {
        public void SetMobId(Guid id);
        public Guid mobId { get; }
    }

    public struct HitInfo
    {
        public int damage;
        public Vector3 position;
        public Vector3 direction;
    }

    public enum HitResponseType
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
                    type = HitResponseType.Ignored,
                    damage = 0
                };
            }
        }

        public HitResponseType type;
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

    public interface IDoor
    {
        public void SetOpen(bool flag);
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
        public const int TEAM_ID_MOBS = 0;
        public const int TEAM_ID_PLAYER = 1;

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

        public static uint GetEnemyProjectileMask()
        {
            return (BIT_WORLD | BIT_PLAYER);
        }
    }
}
