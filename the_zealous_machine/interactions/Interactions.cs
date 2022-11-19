using Godot;
using TheZealousMachine.actors.projectiles;

namespace TheZealousMachine
{
    public static class Interactions
    {

    }

    public interface IGame
    {
        void RegisterPlayer(IPlayer player);
        void UnregisterPlayer(IPlayer player);
        TargetInfo GetPlayerTarget();
        ProjectileGeneric CreateProjectile();
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
}
