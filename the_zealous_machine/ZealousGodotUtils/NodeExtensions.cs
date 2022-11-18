using Godot;

namespace ZealousGodotUtils
{
    public static class NodeExtensions
    {
        public static int GetTotalOverlaps(this Area3D area)
        {
            return area.GetOverlappingAreas().Count() + area.GetOverlappingBodies().Count();
        }

        public static float GetRayFraction(this RayCast3D ray)
        {
            if (!ray.IsColliding()) { return 1; }
            Vector3 near = ray.GlobalTransform.origin;
            Vector3 far = near + ray.TargetPosition;
            float distToFar = near.DistanceTo(far);
            float distToHit = near.DistanceTo(ray.GetCollisionPoint());
            float fraction = distToHit / distToFar;
            return fraction;
        }
    }
}
