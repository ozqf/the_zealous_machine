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
		private PackedScene _bulletCasing = GD.Load<PackedScene>("res://gfx/bullet_casing.tscn");

		// current position tracking target
		private Node3D _subject;
		private AimLaser _target;
		private TurretFormation _formation;
		private float _tick = 0f;
		private TimedVisible _light;
		private TimedVisible _flash;
		private GPUParticles3D _boostParticles;
		private IGame _game;

		private Node3D _spreadTarget = null;
		private Node3D _narrowTarget = null;
		private Node3D _boostTarget = null;

		public override void _Ready()
		{
			_game = Servicelocator.Locate<IGame>();
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
					Rotation = Vector3.Zero;
					_boostParticles.Emitting = false;
					SetTrackTarget(_spreadTarget); break;
				case TurretFormation.Narrow:
					Rotation = Vector3.Zero;
					_boostParticles.Emitting = false;
					SetTrackTarget(_narrowTarget); break;
				case TurretFormation.Boost:
					Rotation = Vector3.Zero;
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

		public void SetAimTarget(AimLaser node)
		{
			_target = node;
		}

		private void _RunMuzzleGFX()
		{
			_light.Run(0.05f);
			_flash.Run(0.05f);
			_flash.RotateZ(GD.RandRange(0, 360) * ZGU.DEG2RAD);
		}

		private void _SpawnBulletCasing()
		{
			var casing = _bulletCasing.Instantiate<RigidBody3D>();
			_game.GetActorRoot().AddChild(casing);
			casing.GlobalTransform = GlobalTransform;
			Vector3 dir = new Vector3(
				(float)GD.RandRange(-0.5f, 0.5f),
				(float)GD.RandRange(-0.5f, 0.5f),
				(float)GD.RandRange(-0.5f, 0.5f)
				);
			casing.LinearVelocity = (GlobalTransform.basis.y + dir) * 10f;
			casing.AngularVelocity = new Vector3(
				(float)GD.RandRange(-3.2, 3.2f),
				(float)GD.RandRange(-3.2, 3.2f),
				(float)GD.RandRange(-3.2, 3.2f)
				);
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
				_RunMuzzleGFX();
				_SpawnBulletCasing();
				_tick = 0.1f;
				IProjectile prj = _game.CreateProjectile();
				ProjectileLaunchInfo info = new ProjectileLaunchInfo();
				info.t = GlobalTransform.MovedForward(0.5f);
				info.speed = 200f;
				info.damage = 8;
				info.teamId = Interactions.TEAM_ID_PLAYER;
				prj.Launch(info);
			}
			else if (Input.IsActionPressed("attack_2"))
			{
				_RunMuzzleGFX();
				_SpawnBulletCasing();
				_tick = 1f;
				Transform3D t = GlobalTransform;
				Vector3 origin = t.origin;
				float spread = 5f * ZGU.DEG2RAD;
				for (int i = 0; i < 10; ++i)
				{
					IProjectile prj = _game.CreateProjectile();
					ProjectileLaunchInfo info = new ProjectileLaunchInfo();
					info.t = GlobalTransform.MovedForward(0.5f);
					info.t.origin = Vector3.Zero;
					info.t = info.t.Rotated(t.basis.x, (float)GD.RandRange(-spread, spread));
					info.t = info.t.Rotated(t.basis.y, (float)GD.RandRange(-spread, spread));
					info.t.origin = origin;
					info.speed = 200f;
					info.damage = 8;
					info.teamId = Interactions.TEAM_ID_PLAYER;
					prj.Launch(info);
				}
			}
		}

		public override void _Process(double delta)
		{
			if (_subject != null)
			{
				GlobalPosition = _subject.GlobalTransform.origin;
			}
			if (_formation == TurretFormation.Spread && _target != null)
			{
				this.LookAtSafe(_target.GetAimPosition(), Vector3.Up, Vector3.Left);
			}
		}
	}
}
