using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.projectiles
{
    public partial class ProjectileGeneric : Node3D
    {
        private float _speed = 50f;
        private RayCast3D _ray;
        private float _timeToLive = 30f;

        public override void _Ready()
        {
            _ray = GetNode<RayCast3D>("RayCast3D");
        }
        public void Launch(ProjectileLaunchInfo launchInfo)
        {
            Transform3D t = this.GlobalTransform;
            t.origin = launchInfo.position;
            t = t.LookingAt(launchInfo.position + launchInfo.forward, Vector3.Up);
            GlobalTransform = t;
            _speed = launchInfo.speed;
        }

        private void _Step(float delta)
        {
            GlobalPosition += (-GlobalTransform.basis.z) * (_speed * delta);
        }

        public override void _PhysicsProcess(double delta)
        {
            float dt = (float)delta;
            _timeToLive -= dt;
            if ( _timeToLive <= 0 )
            {
                this.QueueFree();
                return;
            }
            float fraction = _ray.GetRayFraction();
            if (fraction == 1f)
            {
                // move
                _Step(dt);
                return;
            }
            float stepDist = _speed * dt;
            float weight = stepDist / 10f;
            if (fraction > weight)
            {
                _Step(dt);
                return;
            }
            this.QueueFree();
        }
    }
}
