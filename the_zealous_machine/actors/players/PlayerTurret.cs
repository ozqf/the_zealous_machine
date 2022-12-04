using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.players
{
	public enum TurretFormation
	{
		Spread,
		Narrow,
		Boost,
		LockOn
	}

	public partial class PlayerTurret : Node3D, IItemCollector
	{
		private PackedScene _bulletCasing = GD.Load<PackedScene>("res://gfx/bullet_casing.tscn");

		public enum TurretState { InFormation, Recoiling, Reloading, Launched, Returning }

		private TurretState _state = TurretState.InFormation;
		// current position tracking target
		private Node3D _subject;
		private TurretFormation _formation;
		private ITurretUser _user = null;
		private int _aimPointIndex = 0;
		private bool _longReload = false;
		private float _tick = 0f;
		private float _jerkTick = 0f;
		private float _jerkTime = 0f;
		private Vector3 _jerkOrigin = new Vector3(0, 0, 0.5f);
		private TimedVisible _light;
		private TimedVisible _flash;
		private GPUParticles3D _boostParticles;
		private MeshInstance3D _mesh;
		private IGame _game;

		private Node3D _spreadTarget = null;
		private Node3D _narrowTarget = null;
		private Node3D _boostTarget = null;
		private Node3D _lockOnTarget = null;

		public override void _Ready()
		{
			_game = Servicelocator.Locate<IGame>();
			_light = GetNode<TimedVisible>("light");
			_flash = GetNode<TimedVisible>("muzzle_spikes");
			_boostParticles = GetNode<GPUParticles3D>("booster_particles");
			_mesh = GetNode<MeshInstance3D>("mesh");

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

		public void SetUser(ITurretUser user, int aimPointIndex)
		{
			_user = user;
            _aimPointIndex = aimPointIndex;

        }

		public void SetFormation(TurretFormation formation)
		{
			_formation = formation;
			switch (_formation)
			{
				case TurretFormation.Spread:
					Rotation = Vector3.Zero;
					_boostParticles.Emitting = false;
					Scale = new Vector3(1, 1, 1);
					SetTrackTarget(_spreadTarget); break;
				case TurretFormation.Narrow:
					Rotation = Vector3.Zero;
					_boostParticles.Emitting = false;
                    Scale = new Vector3(0.5f, 0.5f, 0.5f);
                    SetTrackTarget(_narrowTarget); break;
				case TurretFormation.Boost:
					Rotation = Vector3.Zero;
					_boostParticles.Emitting = true;
                    Scale = new Vector3(1, 1, 1);
                    SetTrackTarget(_boostTarget); break;
				case TurretFormation.LockOn:
					Rotation = Vector3.Zero;
					_boostParticles.Emitting = false;
                    Scale = new Vector3(1, 1, 1);
                    SetTrackTarget(_lockOnTarget); break;

			}
		}

		public void SetTrackTargets(Node3D spread, Node3D narrow, Node3D boost, Node3D lockOnNode)
		{
			_spreadTarget = spread;
			_narrowTarget = narrow;
			_boostTarget = boost;
			_lockOnTarget = lockOnNode;
		}

		public void SetTrackTarget(Node3D node)
		{
			_subject = node;
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

			_longReload = false;
			// No firing if in boost mode
			if (_formation == TurretFormation.Boost)
			{
				return;
			}

			if (Input.IsActionPressed("attack_1"))
			{
				_RunMuzzleGFX();
				//_SpawnBulletCasing();
				_tick = 0.1f;
				IProjectile prj = _game.CreateProjectile(ProjectileType.PlayerBasic);
				ProjectileLaunchInfo info = new ProjectileLaunchInfo();
				info.t = GlobalTransform.MovedForward(0.5f);
				info.speed = 200f;
				info.damage = 6;
				info.teamId = Interactions.TEAM_ID_PLAYER;
				prj.Launch(info);
			}
			else if (Input.IsActionPressed("attack_2") && _user.CheckAndTakeItem("energy", 2))
			{
				_jerkTime = 0.3f;
				_jerkTick = 0;
				_state = TurretState.Recoiling;
				_RunMuzzleGFX();
				_SpawnBulletCasing();
				_tick = 1f;
				Transform3D t = GlobalTransform;
				Vector3 origin = t.origin;
				float spread = 5f * ZGU.DEG2RAD;
				for (int i = 0; i < 10; ++i)
				{
					IProjectile prj = _game.CreateProjectile(ProjectileType.PlayerBasic);
					ProjectileLaunchInfo info = new ProjectileLaunchInfo();
					info.t = GlobalTransform.MovedForward(0.5f);
					info.t.origin = Vector3.Zero;
					info.t = info.t.Rotated(t.basis.x, (float)GD.RandRange(-spread, spread));
					info.t = info.t.Rotated(t.basis.y, (float)GD.RandRange(-spread, spread));
					info.t.origin = origin;
					info.speed = 200f;
					info.damage = 10;
					info.teamId = Interactions.TEAM_ID_PLAYER;
					prj.Launch(info);
				}
			}
		}

		public override void _Process(double delta)
		{
			// TODO: This movement is framerate dependent!
			// perhaps only mesh should move like this?
			if (_subject != null)
			{
				Vector3 tPos = _subject.GlobalPosition;
				Vector3 p = GlobalPosition.Lerp(tPos, 0.25f);
				GlobalPosition = p;
			}

			if (_jerkTime > 0)
			{
				
			}

			switch (_state)
			{
				case TurretState.Recoiling:
					_jerkTick += (float)delta;
					if (_jerkTick >= _jerkTime)
					{
						_jerkTick = _jerkTime;
						_state = TurretState.Reloading;
					}
					_mesh.Position = _jerkOrigin.Lerp(Vector3.Zero, _jerkTick / _jerkTime);
					break;
				case TurretState.Reloading:
					this.Rotation = new Vector3(90f * ZGU.DEG2RAD, 0, 0);
					if (_tick <= 0)
					{
						_state = TurretState.InFormation;
					}
					break;
				default:
					if (_longReload)
					{
						this.Rotation = new Vector3(90f * ZGU.DEG2RAD, 0, 0);
						return;
					}
					if (_IsFormationAimed(_formation))
					{
						this.LookAtSafe(_user.GetTurretAimPoint(_aimPointIndex), Vector3.Up, Vector3.Left);
					}
					break;
			}
		}

		private bool _IsFormationAimed(TurretFormation formation)
		{
			if (formation == TurretFormation.Boost) { return false; }
			return true;
		}

		public int GiveItem(string type, int count)
		{
			return _user.GetItemCollector().GiveItem(type, count);
		}

		public int CanTake(string type, int count)
		{
			return _user.GetItemCollector().CanTake(type, count);
		}
	}
}
