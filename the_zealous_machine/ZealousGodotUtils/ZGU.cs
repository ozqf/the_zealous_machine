using Godot;

namespace ZealousGodotUtils
{
    public static class ZGU
    {
        public const float DEG2RAD = 0.017453292519f;
        public const float RAD2DEG = 57.29577951308f;
        public const string ROOT_DIR = "res://";

        public static int RandomIndex(int length, float seed)
        {
            return (int)Mathf.Floor(seed * length);
        }
    }
}
