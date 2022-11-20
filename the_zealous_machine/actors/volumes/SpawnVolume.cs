using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.volumes
{
	public partial class SpawnVolume : RigidBody3D
	{
		private float _tick = 0.5f;
		public override void _PhysicsProcess(double delta)
		{
			_tick -= (float)delta;
			if (_tick <= 0 )
			{
				_tick = 9999;
				Servicelocator.Locate<IGame>().CreateMobDrone(GlobalPosition);
				QueueFree();
			}
		}
	}
}
