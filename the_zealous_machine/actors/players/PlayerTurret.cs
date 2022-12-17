using Godot;
using TheZealousMachine.shared.scripts;
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

		public enum TurretState { Idle, Recoiling, Reloading, Launched, Stuck, Returning }

		private TurretState _state = TurretState.Idle;
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
		private TimedVisible _flash2;
		private GPUParticles3D _boostParticles;
		private MeshInstance3D _mesh;
		private Node3D _shockCone;
		private HurtArea _hurtArea;
		private IGame _game;

		private float _flightTime = 0.25f;
		private float _flightSpeed = 100f;
		private Vector3 _returnOrigin = new Vector3();

		private Node3D _spreadTarget = null;
		private Node3D _narrowTarget = null;
		private Node3D _boostTarget = null;
		private Node3D _lockOnTarget = null;

		public TurretState turretStatue { get { return _state; } }

		public bool inFormation
		{
			get
			{
				return _state == TurretState.Idle
					|| _state == TurretState.Recoiling
					|| _state == TurretState.Reloading;
			}
		}

		public override void _Ready()
		{
			_game = Servicelocator.Locate<IGame>();
			_light = GetNode<TimedVisible>("light");
			_flash = GetNode<TimedVisible>("muzzle_spikes");
			_flash2 = GetNode<TimedVisible>("muzzle_spikes2");
			_boostParticles = GetNode<GPUParticles3D>("booster_particles");
			_mesh = GetNode<MeshInstance3D>("mesh");
			_shockCone = GetNode<Node3D>("shock_cone");
			_hurtArea = GetNode<HurtArea>("hurt_area");
			_hurtArea.SetOn(false);
			_shockCone.Visible = false;

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

		private void _RunReverseMuzzleGFX()
		{
			_flash2.Run(0.05f);
			_flash2.RotateZ(GD.RandRange(0, 360) * ZGU.DEG2RAD);
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

		private void _RunReload()
		{

		}

		public override void _PhysicsProcess(double delta)
		{
			switch (_state)
			{
				case TurretState.Launched:
					if (GetTree().GetFrame() % 2 == 0)
					{
						_flash2.RotateZ(GD.RandRange(0, 360) * ZGU.DEG2RAD);
					}
					break;
			}
			if (!inFormation)
			{
				return;

			}
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
			/*else if (Input.IsActionPressed("attack_2") && _user.CheckAndTakeItem("energy", 2))
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
					info.hyper = 1;
					prj.Launch(info);
				}
			}*/
		}

		public void Launch()
		{
			GD.Print($"Turret {Name} launch");
			_state = TurretState.Launched;
			_boostParticles.Emitting = true;
			_shockCone.Visible = true;
			_SpawnBulletCasing();
			_tick = 0;
			_flightSpeed = 200f;
			_flightTime = 0.1f;
			_hurtArea.SetOn(true);
			//_RunReverseMuzzleGFX();
			_flash2.Visible = true;
			_flash2.RotateZ(GD.RandRange(0, 360) * ZGU.DEG2RAD);
			//Transform3D t = GlobalTransform;
			//Vector3 fxPos = t.origin - (t.Forward());
			//_game.CreateMuzzleFlash(fxPos, t.basis.z);
		}

		private void _MoveLaunched(float delta)
		{
			_tick += delta;
			Vector3 step = this.GlobalTransform.Forward() * _flightSpeed * delta;
			Vector3 from = GlobalPosition;
			Vector3 to = from + step;
			
			PhysicsRayQueryParameters3D ray = new PhysicsRayQueryParameters3D();
			ray.CollideWithAreas = false;
			ray.CollideWithBodies = true;
			ray.From = from;
			ray.To = to;
			ray.CollisionMask = 1;
			PhysicsDirectSpaceState3D space = GetWorld3d().DirectSpaceState;
			Godot.Collections.Dictionary result = space.IntersectRay(ray);
			
			if (result.Count > 0)
			{
				_state = TurretState.Stuck;
				_flightTime = 1f;
				GlobalPosition = (Vector3)result["position"];
				_shockCone.Visible = false;
				_returnOrigin = GlobalPosition;
				_boostParticles.Emitting = false;
				_flash2.Visible = false;
				GD.Print("Hid flash 2");
				return;
			}
			GlobalPosition = to;

			if (_tick >= _flightTime)
			{
				_boostParticles.Emitting = false;
				_tick = 0f;
				_flightTime = 4f;
				_state = TurretState.Returning;
				_shockCone.Visible = false;
				_returnOrigin = GlobalPosition;
				_flash2.Visible = false;
				_hurtArea.SetOn(false);
			}
		}

		private void _MoveStuck(float delta)
		{
			_tick += delta;
			if (_tick >= _flightTime)
			{
				_tick = 0f;
				_flightTime = 3f;
				_state = TurretState.Returning;
				_returnOrigin = GlobalPosition;
				_hurtArea.SetOn(false);
			}
		}

		private void _MoveReturning(float delta)
		{
			_tick += delta;
			GlobalPosition = _returnOrigin.Lerp(_subject.GlobalPosition, _tick / _flightTime);
			if (_tick >= _flightTime)
			{
				_tick = 0;
				_state = TurretState.Idle;
			}
		}

		private void _MoveInFormation(float delta)
		{
			// TODO: This movement is framerate dependent!
			// perhaps only mesh should move like this?
			if (_subject != null)
			{
				Vector3 tPos = _subject.GlobalPosition;
				Vector3 p = tPos;
				//Vector3 p = GlobalPosition.Lerp(tPos, 0.25f);
				GlobalPosition = p;
			}
		}

		public override void _Process(double delta)
		{
			// handle positioning
			switch (_state)
			{
				case TurretState.Launched:
					_MoveLaunched((float)delta);
					return;
				case TurretState.Stuck:
					_MoveStuck((float)delta);
					return;
				case TurretState.Returning:
					_MoveReturning((float)delta);
					return;
				default:
					_MoveInFormation((float)delta);
					break;
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
						_state = TurretState.Idle;
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
