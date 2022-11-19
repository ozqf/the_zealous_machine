using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.players
{
    public partial class PlayerTurret : Node3D
    {
        private Node3D _subject;
        private float _tick = 0f;

        public override void _Ready()
        {
            Node player = this.FindParentOfTypeRecursive<IPlayer>();
            if (player != null)
            {
                GD.Print("Turret found player: " + player.Name);
            }
            else
            {
                GD.Print("Turret found no player!");
            }
        }

        public void SetTrackTarget(Node3D node)
        {
            _subject = node;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_tick > 0f)
            { 
                _tick -= (float)delta;
                return;
            }
            if (Input.IsActionPressed("attack_1"))
            {
                _tick = 0.1f;
                var prj = Servicelocator.Locate<Main>().CreateProjectile();
                ProjectileLaunchInfo info = new ProjectileLaunchInfo();
                info.forward = -GlobalTransform.basis.z;
                info.position = GlobalPosition + (info.forward * 0.5f);
                info.speed = 80f;
                prj.Launch(info);
            }
        }

        public override void _Process(double delta)
        {
            if (_subject != null)
            {
                GlobalPosition = _subject.GlobalTransform.origin;
            }
        }
    }
}
