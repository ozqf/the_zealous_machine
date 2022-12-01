using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.players
{
	public partial class PlayerWreck : RigidBody3D
	{
		private Camera3D _camera;

		public override void _Ready()
		{
			_camera = GetNode<Camera3D>("Camera3D");
		}

		public void Launch(Transform3D wreckT, Transform3D cameraT, Vector3 velocity)
		{
			GlobalTransform = wreckT;
			_camera.GlobalTransform = cameraT;
			if (velocity.LengthSquared() < 1f)
			{
				velocity.x = (float)GD.RandRange(-15f, 15f);
				velocity.y = (float)GD.RandRange(-15f, 15f);
				velocity.z = (float)GD.RandRange(-15f, 15f);
			}
			
			LinearVelocity = velocity;
			AngularVelocity = new Vector3(
				(float)GD.RandRange(-3.2, 3.2f),
				(float)GD.RandRange(-3.2, 3.2f),
				(float)GD.RandRange(-3.2, 3.2f)
				);
		}

		public override void _Process(double delta)
		{
			_camera.LookAtSafe(GlobalPosition, Vector3.Up, Vector3.Left);
		}
	}
}
