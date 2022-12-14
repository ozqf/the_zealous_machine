using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.shared.scripts
{
	public partial class Rotator3D : Node3D
	{
		[Export]
		public Vector3 degreesPerSecond = new Vector3();

		public override void _PhysicsProcess(double delta)
		{
			this.Rotation += (degreesPerSecond * ZGU.DEG2RAD) * (float)delta;
		}
	}
}
