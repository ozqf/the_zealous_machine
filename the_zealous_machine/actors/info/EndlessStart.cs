using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.info
{
	public partial class EndlessStart : Node3D
	{
		private int _tick = 2;
		public override void _Ready()
		{
			Visible = false;
		}

		public override void _PhysicsProcess(double delta)
		{
			_tick -= 1;
			if (_tick <= 0)
			{
				_tick = 999999;
				SetProcess(false);
				Servicelocator.Locate<IGame>().SpawnNextRoom(this.GlobalTransform);
			}
		}
	}
}
