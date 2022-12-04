using Godot;
using System;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.volumes
{
	public partial class SpawnVolume : RigidBody3D
	{
		public Guid mobId = Guid.Empty;
		public MobType mobType = 0;
		private float _tick = 0.5f;
		public const float totalTime = 1.5f;

		public override void _Ready()
		{
			_tick = totalTime;
		}

		public override void _PhysicsProcess(double delta)
		{
			_tick -= (float)delta;
			if (_tick <= 0)
			{
				_tick = 9999;
				IMob mob = Servicelocator.Locate<IGame>().CreateMob(GlobalPosition, mobType);
				if (mob != null)
				{
					mob.SetMobId(mobId);
				}
				SetPhysicsProcess(false);
				QueueFree();
			}
			else
			{
				float weight = 1f - (_tick / totalTime);
				this.Scale = new Vector3(0.1f, 0.1f, 0.1f).Lerp(new Vector3(3, 3, 3), weight);
			}
		}
	}
}
