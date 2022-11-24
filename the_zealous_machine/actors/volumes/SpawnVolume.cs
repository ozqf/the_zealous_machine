using Godot;
using System;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.volumes
{
	public partial class SpawnVolume : RigidBody3D
	{
		public Guid mobId = Guid.Empty;
		public int mobType = 0;
		private float _tick = 0.5f;
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
				QueueFree();
			}
		}
	}
}
