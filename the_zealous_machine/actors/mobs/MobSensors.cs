using Godot;

namespace TheZealousMachine.actors.mobs
{
	public partial class MobSensors : Node3D
	{
		private PhysicsPointQueryParameters3D pointTest = new();

		public int overlaps = -1;

		public override void _PhysicsProcess(double delta)
		{
			pointTest.Position = GlobalPosition;
			pointTest.CollisionMask = (1 << 10);
			pointTest.CollideWithAreas = true;
			
			PhysicsDirectSpaceState3D space = GetWorld3d().DirectSpaceState;
			overlaps = space.IntersectPoint(pointTest).Count;
		}
	}
}
