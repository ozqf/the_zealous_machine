using Godot;

namespace TheZealousMachine.actors.mobs
{
    public class MobThinkInfo
    {
        public TargetInfo targetInfo = new TargetInfo();
        public Transform3D t;
        public Vector3 toward;
        public float distSqr;
        public bool canSeeTarget = true;

        public void Refresh(Node3D self, IGame game)
        {
            targetInfo = game.GetPlayerTarget();
            t = self.GlobalTransform;
            if (!targetInfo.valid) { return; }

            distSqr = t.origin.DistanceSquaredTo(targetInfo.position);
            toward = (targetInfo.position - t.origin).Normalized();
            canSeeTarget = game.CheckLoS(t.origin, targetInfo.position);
        }

        public bool HasTarget()
        {
            return targetInfo.valid;
        }
    }
}
