using Godot;
using TheZealousMachine.shared.scripts;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.mobs.turrets
{
	public partial class MobTurret : CharacterBody3D
	{
		[Export]
		public bool lookAtTarget = true;
		[Export]
		public Vector3 degreesPerSecond = new Vector3();
		[Export]
		public bool trackX = true;
		[Export]
		public bool trackY = true;
		[Export]
		public bool trackZ = true;
		
		private IGame _game;
		private Node3D _barrels;

		public override void _Ready()
		{
			_barrels = this.FindFirstChildOfType<Rotator3D>();
			if (_barrels == null)
			{
				_barrels = GetNodeOrNull<Node3D>("barrels");
			}
			_game = Servicelocator.Locate<IGame>();
		}

		public override void _Process(double delta)
		{
			TargetInfo info = _game.GetPlayerTarget();
			if (!info.valid)
			{
				return;
			}

			Vector3 p = GlobalPosition;
			Vector3 lookP = p;
			if (trackX) { lookP.x = info.position.x; }
			if (trackY) { lookP.y = info.position.y; }
			if (trackZ) { lookP.z = info.position.z; }
			if (lookP != p)
			{
				this.LookAtSafe(lookP, Vector3.Up, Vector3.Right);
			}

			if (_barrels != null)
			{
				//Vector3 rot = new Vector3(90f, 90f, 0);
				_barrels.Rotation += (degreesPerSecond * ZGU.DEG2RAD) * (float)delta;
				//GD.Print($"rot {_barrels.Rotation}");
			}
		}
	}
}
