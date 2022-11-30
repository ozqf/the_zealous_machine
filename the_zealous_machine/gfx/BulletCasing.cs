using Godot;

namespace TheZealousMachine.gfx
{
	public partial class BulletCasing : RigidBody3D
	{
		private static int _count;

		public static int GetCount() {  return _count; }

		[Export]
		public float tick = 10f;

		public override void _Ready()
		{
			_count += 1;
		}

		public override void _ExitTree()
		{
			_count -= 1;
		}

		public override void _PhysicsProcess(double delta)
		{
			tick -= (float)delta;
			if (tick <= 0)
			{
				tick = 999999;
				QueueFree();
			}
		}
	}
}
