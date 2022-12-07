using Godot;

namespace TheZealousMachine.actors.projectiles
{
    public partial class PrjLightHide : Node3D
    {
        [Export]
        public int frameModulo = 2;

        public override void _Ready()
        {
            if (frameModulo <= 0) { return; }
            if (GetTree().GetFrame() % frameModulo != 0)
            {
                Visible = false;
            }
        }
    }
}
