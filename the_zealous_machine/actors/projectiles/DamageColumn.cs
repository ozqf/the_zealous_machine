using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.projectiles
{
	public partial class DamageColumn : RayCast3D
	{
		private IHittable _hittable;
		private float _hurtTick = 0f;
		private HitInfo _hit;

		public override void _Ready()
		{
			_hit = new HitInfo();
			_hit.damage = 10;
			Area3D area = GetNode<Area3D>("Area3D");
			area.BodyEntered += _OnBodyEntered;
			area.BodyExited += _OnBodyExited;
		}

		private void _OnBodyEntered(Node3D body)
		{
			GD.Print("Body entered column");
			IHittable hittable = body as IHittable;
			if (hittable != null)
			{
				_hittable = hittable;
			}
		}

		private void _OnBodyExited(Node3D body)
		{
			GD.Print("Body exited column");
			IHittable hittable = body as IHittable;
			if (hittable != null)
			{
				if (hittable == _hittable)
				{
					_hittable = null;
				}
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			float range = TargetPosition.Length();
			float fraction = this.GetRayFraction();
			this.Scale = new Vector3(0.5f, 0.5f, range * fraction);
			if (_hittable != null)
			{
				_hurtTick -= (float)delta;
				if (_hurtTick <= 0f)
				{
					_hurtTick = 0.2f;
					_hittable.Hit(_hit);
				}
			}
		}
	}
}
