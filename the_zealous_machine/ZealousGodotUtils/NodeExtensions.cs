using Godot;

namespace ZealousGodotUtils
{
    public static class NodeExtensions
    {
        public static int GetTotalOverlaps(this Area3D area)
        {
            return area.GetOverlappingAreas().Count() + area.GetOverlappingBodies().Count();
        }

        public static Vector3 ClampMagnitude(this Vector3 v, float magnitude)
        {
            if (v.LengthSquared() < magnitude * magnitude) { return v; }
            return v.Normalized() * magnitude;
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

        public static uint GetColliderMask(this RayCast3D ray)
        {
            if (!ray.IsColliding()) { return 0; }
            CollisionObject3D obj = ray.GetCollider() as CollisionObject3D;
            return obj != null ? obj.CollisionMask : 0;
        }

        public static Node? FindParentOfTypeRecursive<T>(this Node node)
        {
            Node parent = node.GetParent();
            if (parent is T)
            {
                return parent as Node;
            }
            // oh dear we have no more nodes to search
            if (parent == node.GetTree().Root)
            {
                return null;
            }
            return FindParentOfTypeRecursive<T>(parent);
        }
    }
}
