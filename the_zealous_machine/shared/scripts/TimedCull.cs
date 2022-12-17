using Godot;

namespace TheZealousMachine.shared.scripts
{
    public partial class TimedCull : Node3D
    {
        [Export]
        public float timeToLive = 1;

        public override void _PhysicsProcess(double delta)
        {
            timeToLive -= (float)delta;
            if (timeToLive <= 0f)
            {
                timeToLive = 999;
                SetPhysicsProcess(false);
                QueueFree();
            }
        }
    }
}
