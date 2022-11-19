using Godot;

namespace TheZealousMachine
{
    public static class Interactions
    {

    }

    public interface IArena
    {
        public void TriggerTouched(string name, string message);
    }

    public interface IPlayer
    {
        TargetInfo GetTargetInfo();
    }

    public struct TargetInfo
    {
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
