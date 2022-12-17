using Godot;

namespace ZealousGodotUtils
{
    public static class NodeExtensions
    {
        public static void LookAtSafe(this Node3D node, Vector3 target, Vector3 up, Vector3 alternateUp)
        {
            if (node == null) { return; }
            //if (node.GlobalPosition == target) { return; }
            if (node.GlobalPosition.IsEqualApprox(target)) { return; }
            Vector3 toward = (target - node.GlobalPosition).Normalized();
            float dot = toward.Dot(up);
            if (dot >= 1f || dot <= -1f)
            {
                up = alternateUp;
            }
            node.LookAt(target, up);
        }

        public static Vector3 Forward(this Transform3D t) { return -t.basis.z; }

        public static Transform3D MovedForward(this Transform3D t, float distance)
        {
            t.origin += (-t.basis.z) * distance;
            return t;
        }

        public static Vector3 ForwardGlobal(this Node3D node)
        {
            return -node.GlobalTransform.basis.z;
        }

        public static Transform3D RotateAtOrigin(this Transform3D t, Vector3 axis, float radians)
        {
            Vector3 origin = t.origin;
            t.origin = Vector3.Zero;
            t = t.Rotated(axis, radians);
            t.origin = origin;
            return t;
        }

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

        // TODO: Doesn't work for interfaces, only classes
        public static T FindFirstChildOfType<T>(this Node node)
        {
            int numChildren = node.GetChildCount();
            for (int i = 0; i < numChildren; i++)
            {
                Node child = node.GetChild(i);
                if (child is T)
                {
                    T obj = (T)Convert.ChangeType(child, typeof(T));
                    return obj;
                }
            }
            return default(T);
        }

        // TODO: Doesn't work for interfaces, only classes
        public static List<T> FindChildrenOfType<T>(this Node node)
        {
            List<T> values = new List<T>();
            int numChildren = node.GetChildCount();
            for (int i = 0; i < numChildren; i++)
            {
                Node child = node.GetChild(i);
                if (child is T)
                {
                    T obj = (T)Convert.ChangeType(child, typeof(T));
                    values.Add(obj);
                }
            }
            return values;
        }
    }
}
