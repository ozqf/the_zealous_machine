using Godot;
using System;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.volumes
{
	public partial class SpawnVolume : RigidBody3D
	{
		public Guid mobId = Guid.Empty;
		private float _tick = 0.5f;
		public override void _PhysicsProcess(double delta)
		{
			_tick -= (float)delta;
			if (_tick <= 0)
			{
				_tick = 9999;
				IMob mob = Servicelocator.Locate<IGame>().CreateMobDrone(GlobalPosition);
				if (mob != null)
				{
					mob.SetMobId(mobId);
				}
				QueueFree();
			}
		}
	}
}
