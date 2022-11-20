using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.players
{
	public enum TurretFormation
	{
		Spread,
		Narrow,
		Boost
	}

	public partial class PlayerTurret : Node3D
	{
		// current position tracking target
		private Node3D _subject;
		private TurretFormation _formation;
		private float _tick = 0f;
		private TimedVisible _light;
		private TimedVisible _flash;
		private GPUParticles3D _boostParticles;

		private Node3D _spreadTarget = null;
		private Node3D _narrowTarget = null;
		private Node3D _boostTarget = null;

		public override void _Ready()
		{
			_light = GetNode<TimedVisible>("light");
			_flash = GetNode<TimedVisible>("muzzle_spikes");
			_boostParticles = GetNode<GPUParticles3D>("booster_particles");

			Node player = this.FindParentOfTypeRecursive<IPlayer>();
			if (player != null)
			{
				GD.Print("Turret found player: " + player.Name);
			}
			else
			{
				GD.Print("Turret found no player!");
			}
		}

		public void SetFormation(TurretFormation formation)
		{
			_formation = formation;
			switch (_formation)
			{
				case TurretFormation.Spread:
					_boostParticles.Emitting = false;
					SetTrackTarget(_spreadTarget); break;
				case TurretFormation.Narrow:
					_boostParticles.Emitting = false;
					SetTrackTarget(_narrowTarget); break;
				case TurretFormation.Boost:
					_boostParticles.Emitting = true;
					SetTrackTarget(_boostTarget); break;
			}
		}

		public void SetTrackTargets(Node3D spread, Node3D narrow, Node3D boost)
		{
			_spreadTarget = spread;
			_narrowTarget = narrow;
			_boostTarget = boost;
		}

		public void SetTrackTarget(Node3D node)
		{
			_subject = node;
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_tick > 0f)
			{ 
				_tick -= (float)delta;
				return;
			}

			// No firing if in boost mode
			if (_formation == TurretFormation.Boost)
			{
				return;
			}

			if (Input.IsActionPressed("attack_1"))
			{
				_light.Run(0.05f);
				_flash.Run(0.05f);
				_flash.RotateZ(GD.RandRange(0, 360) * ZGU.DEG2RAD);
				_tick = 0.1f;
				var prj = Servicelocator.Locate<IGame>().CreateProjectile();
				ProjectileLaunchInfo info = new ProjectileLaunchInfo();
				info.forward = -GlobalTransform.basis.z;
				info.position = GlobalPosition + (info.forward * 0.5f);
				info.speed = 80f;
				prj.Launch(info);
			}
		}

		public override void _Process(double delta)
		{
			if (_subject != null)
			{
				GlobalPosition = _subject.GlobalTransform.origin;
			}
		}
	}
}
