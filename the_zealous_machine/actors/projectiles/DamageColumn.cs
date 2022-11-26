using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.projectiles
{
	public partial class DamageColumn : RayCast3D
	{
		public override void _PhysicsProcess(double delta)
		{
			float range = TargetPosition.Length();
			float fraction = this.GetRayFraction();
			this.Scale = new Vector3(0.5f, 0.5f, range * fraction);
		}
	}
}
