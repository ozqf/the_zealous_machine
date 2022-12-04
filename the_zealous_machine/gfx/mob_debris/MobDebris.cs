using Godot;
using System.Collections.Generic;
using ZealousGodotUtils;

namespace TheZealousMachine
{
	public partial class MobDebris : Node3D
	{
		float _tick = 10f;

		public void Launch()
		{
			SetProcess(true);
			Vector3 forward = GlobalTransform.Forward();
			List<RigidBody3D> list = this.FindChildrenOfType<RigidBody3D>();
			float offsetMagnitude = 1.5f;
			foreach (RigidBody3D r in list)
			{
				Vector3 offset = new Vector3(
					(float)GD.RandRange(-offsetMagnitude, offsetMagnitude),
					(float)GD.RandRange(-offsetMagnitude, offsetMagnitude),
					(float)GD.RandRange(-offsetMagnitude, offsetMagnitude)
					);
				float speed = (float)GD.RandRange(10f, 50f);
				r.LinearVelocity = (forward + offset).Normalized() * speed;
				Vector3 rot = new Vector3(
					(float)GD.RandRange(-Mathf.Pi, Mathf.Pi),
					(float)GD.RandRange(-Mathf.Pi, Mathf.Pi),
					(float)GD.RandRange(-Mathf.Pi, Mathf.Pi)
					);
				r.AngularVelocity = rot;
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			_tick -= (float)delta;
			if (_tick <= 0)
			{
				_tick = 99999;
				SetPhysicsProcess(false);
				QueueFree();
				return;
			}
		}
	}
}
