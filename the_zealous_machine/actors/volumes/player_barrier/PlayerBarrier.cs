using Godot;

namespace TheZealousMachine
{
	internal partial class PlayerBarrier : StaticBody3D
	{
		[Export]
		public bool on = false;
		private CollisionShape3D _shape;
		//private bool _dirty = false;

		public override void _Ready()
		{
			_shape = GetNode<CollisionShape3D>("CollisionShape3D");
			SetOn(on);
		}

		public void SetOn(bool flag)
		{
			on = flag;
			//_dirty = true;
			SetProcess(true);
			//_shape.Disabled = !on;
			//Visible = on;
		}

		public override void _PhysicsProcess(double delta)
		{
			_shape.Disabled = !on;
			Visible = on;
			SetProcess(false);
		}
	}
}
