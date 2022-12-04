using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheZealousMachine.actors.players;
using TheZealousMachine.actors.volumes;
using TheZealousMachine.gfx;
using ZealousGodotUtils;

namespace TheZealousMachine
{
	internal struct PlayerMoveSettings
	{
		public static PlayerMoveSettings Get(TurretFormation formation)
		{
			switch (formation)
			{
				case TurretFormation.Boost:
					return new PlayerMoveSettings
					{
						pushForce = 100f,
						maxSpeed = 30f,
						drag = 0.9f
					};
				case TurretFormation.Narrow:
					return new PlayerMoveSettings
					{
						pushForce = 200f,
						maxSpeed = 15f,
						drag = 0.8f
					};
				default:
					return new PlayerMoveSettings
					{
						pushForce = 160f,
						maxSpeed = 20f,
						drag = 0.9f
					};
			}
		}

		public float maxSpeed;
		public float drag;
		public float pushForce;
	}

	public partial class PlayerSixDegrees : CharacterBody3D, IPlayer, IHittable, IItemCollector, ITurretUser
	{
		private IGame _game;

		Vector3 _origin;
		Vector3 _angularVelocity = Vector3.Zero;
		private PlayerMoveSettings _moveSettings;
		float _gravityStrength = 20f;
		private float boostGauge = 0f;
		private float boostPerSecond = 50f;
		private Label _debugText;
		private StringBuilder _debugStr = new StringBuilder();
		private Area3D _nearSurface;
		
		private Node3D _bodyMeshes;
		private Node3D _gatlingNode;
		private Node3D _boostNode;
		private Node3D _lockOnLeftNode;
		private Node3D _lockOnRightNode;

		private RaycastPylon _cameraPylon;
		private AimLaser _aimLaser;
		private HudStatus _hudStatus = new HudStatus();
		private TimedVisible _shieldMesh;
		private Vector3 _aimPosition = new Vector3();

		private bool _targetLocked = false;

		private int _health = 100;
		private int _energy = 0;

		private TurretFormation _formation = TurretFormation.Boost;
		private PlayerTurret _centreTurret;
		private List<PlayerTurret> _remoteTurrets = new List<PlayerTurret>();
		private List<Node3D> _spreadTurretPositions = new List<Node3D>();
		private List<Node3D> _narrowTurretPositions = new List<Node3D>();
		private List<Node3D> _boostTurretPositions = new List<Node3D>();

		public override void _Ready()
		{
			_cameraPylon = GetNode<RaycastPylon>("head/raycast_pylon");
			_bodyMeshes = GetNode<Node3D>("body_meshes");
			_shieldMesh = GetNode<TimedVisible>("body_meshes/shield_mesh");
			_shieldMesh.flash = true;
			_nearSurface = GetNode<Area3D>("near_surface");
			_debugText = GetNode<Label>("debug_print");
			_origin = GlobalTransform.origin;

			_gatlingNode = GetNode<Node3D>("head/options/gatling_node");
			_boostNode = GetNode<Node3D>("head/options/boost_node");
			_lockOnLeftNode = GetNode<Node3D>("head/options/lock_on_left_pylon/far");
			_lockOnRightNode = GetNode<Node3D>("head/options/lock_on_right_pylon/far");

			_aimLaser = GetNode<AimLaser>("head/raycast_pylon/target/aim_laser");

			_centreTurret = GetNode<PlayerTurret>("head/options/turret_centre");

			_remoteTurrets.Add(GetNode<PlayerTurret>("head/options/player_turret"));
			_remoteTurrets.Add(GetNode<PlayerTurret>("head/options/player_turret2"));
			_remoteTurrets.Add(GetNode<PlayerTurret>("head/options/player_turret3"));
			_remoteTurrets.Add(GetNode<PlayerTurret>("head/options/player_turret4"));
			
			_narrowTurretPositions.Add(GetNode<Node3D>("head/options/gatling_node/target"));
			_narrowTurretPositions.Add(GetNode<Node3D>("head/options/gatling_node/target2"));
			_narrowTurretPositions.Add(GetNode<Node3D>("head/options/gatling_node/target3"));
			_narrowTurretPositions.Add(GetNode<Node3D>("head/options/gatling_node/target4"));

			_spreadTurretPositions.Add(GetNode<Node3D>("head/options/raycast_pylon/target"));
			_spreadTurretPositions.Add(GetNode<Node3D>("head/options/raycast_pylon2/target"));
			_spreadTurretPositions.Add(GetNode<Node3D>("head/options/raycast_pylon3/target"));
			_spreadTurretPositions.Add(GetNode<Node3D>("head/options/raycast_pylon4/target"));

			_boostTurretPositions.Add(GetNode<Node3D>("head/options/boost_node/1"));
			_boostTurretPositions.Add(GetNode<Node3D>("head/options/boost_node/2"));
			_boostTurretPositions.Add(GetNode<Node3D>("head/options/boost_node/3"));
			_boostTurretPositions.Add(GetNode<Node3D>("head/options/boost_node/4"));
			Color c = Colors.Yellow;

			var lockOnNodes = new List<Node3D>(4);
			lockOnNodes.Add(GetNode<Node3D>("head/options/lock_on_left_pylon/far/upper"));
			lockOnNodes.Add(GetNode<Node3D>("head/options/lock_on_right_pylon/far/upper"));
			lockOnNodes.Add(GetNode<Node3D>("head/options/lock_on_left_pylon/far/lower"));
			lockOnNodes.Add(GetNode<Node3D>("head/options/lock_on_right_pylon/far/lower"));

			int turretCount = _remoteTurrets.Count;
			for (int i = 0; i < turretCount; i++)
			{
				_remoteTurrets[i].SetTrackTargets(
					_spreadTurretPositions[i],
					_narrowTurretPositions[i],
					_boostTurretPositions[i],
					lockOnNodes[i]);
				_remoteTurrets[i].SetUser(this, 0);
			}

			_centreTurret.SetUser(this, 1);


			SetFormation(TurretFormation.Spread);

			// recursive search doesn't seem to work... not sure what I'm doing wrong?
			//Godot.Collections.Array<Node> turrets = FindChildren("player_turret", "", true, false);
			//List<PlayerTurret> turrets = FindChildren("*player_turret*", "", true, false).Select(x => x as PlayerTurret).ToList();
			GD.Print($"Player found {_remoteTurrets.Count} turrets");

			this.TreeExiting += _OnTreeExiting;
			_game = Servicelocator.Locate<IGame>();
			_game.RegisterPlayer(this);
			GlobalEvents.Register(_OnGlobalEvent);
		}

		public IItemCollector GetItemCollector() { return this; }


		public int GetItemCount(string name)
		{
			switch (name)
			{
				case "energy":
					return _energy;
			}
			return 0;
		}

		public void TakeItem(string name, int count)
		{
			switch (name)
			{
				case "energy":
					_energy -= count;
					if (_energy < 0) { _energy = 0; }
					break;
			}
		}

		public bool CheckAndTakeItem(string name, int count)
		{
			switch (name)
			{
				case "energy":
					if (_energy < count) { return false; }
					_energy -= count;
					return true;
			}
			return false;
		}

		public override void _ExitTree()
		{
			GlobalEvents.Unregister(_OnGlobalEvent);
		}

		private void _OnGlobalEvent(string message, object data)
		{
			//GD.Print($"Player saw global event '{message}'");
		}

		private void _OnTreeExiting()
		{
			_game.UnregisterPlayer(this);
		}

		private void SetFormation(TurretFormation formation)
		{
			_formation = formation;
			int turretCount = _remoteTurrets.Count;
			for (int i = 0; i < turretCount; i++)
			{
				_remoteTurrets[i].SetFormation(formation);
			}
			_moveSettings = PlayerMoveSettings.Get(formation);
			if (_formation == TurretFormation.Narrow)
			{
				_cameraPylon.OverrideFarPositionTarget(GetNode<Node3D>("head/raycast_pylon/close"));
			}
			else
			{
				_cameraPylon.OverrideFarPositionTarget(GetNode<Node3D>("head/raycast_pylon/far"));
			}
		}

		public Vector3 GetTurretAimPoint(int index)
		{
			if (index == 1)
			{
				return _aimLaser.GetAimPosition();
			}
			return _aimPosition;
		}


		public int GiveItem(string type, int count)
		{
			if (count <= 0) { return 0; }
			if (type == "energy")
			{
				_energy += count;
				return count;
			}
			return 0;
		}

		public int CanTake(string type, int count)
		{
			if (_energy < 100) { return count; }
			return 0;
		}

		private void _Die()
		{
			_health = 999999;
			QueueFree();
			PackedScene scene = GD.Load<PackedScene>("res://actors/players/player_wreck.tscn");
			PlayerWreck wreck = scene.Instantiate<PlayerWreck>();
			_game.GetActorRoot().AddChild(wreck);
			Camera3D cam = GetNode<Camera3D>("head/raycast_pylon/target/Camera3D");
			wreck.Launch(GlobalTransform, cam.GlobalTransform, Velocity);
		}

		public HitResponse Hit(HitInfo hit)
		{
			GD.Print($"Player: ow for {hit.damage}");
			_health -= hit.damage;
			if (_health <= 0)
			{
				_Die();
			}
			_shieldMesh.Run(1.5f);
			HitResponse response = new HitResponse();
			response.type = HitResponseType.Damaged;
			response.damage = hit.damage;
			return response;
		}

		private void AddTurret(string turretPath, string targetPath)
		{

		}

		public TargetInfo GetTargetInfo()
		{
			TargetInfo info = new TargetInfo();
			info.valid = true;
			info.position = this.GlobalTransform.origin;
			return info;
		}

		private void _RefreshDebugText()
		{
			_debugStr.Clear();
			_debugStr.Append($"{Engine.GetFramesPerSecond()} fps\n");
			_debugStr.Append($"{_health} Health\n");
			_debugStr.Append($"{_energy} Energy\n");
			_debugStr.Append($"Ejected brasss: {BulletCasing.GetCount()}");
			//_debugStr.Append($"Formation: {_formation}\n");
			//_debugStr.Append($"Push max {GetMaxPushSpeed()} push force {GetPushForce()}\n");
			//_debugStr.Append($"Boost: {boostGauge}\n");
			//_debugStr.Append($"Speed: {Velocity.Length()}\nVelocity: {Velocity}\n");
			//_debugStr.Append($"Angular Velocity: {_angularVelocity}\n");
			_debugText.Text = _debugStr.ToString();
		}

		private Vector3 _CalcGravity(Vector3 velocityNormal, float delta)
		{
			// are we moving with or against gravity?
			float gravityDot = Vector3.Down.Dot(Velocity.Normalized());
			if (gravityDot == 0)
			{
				return Vector3.Zero;
			}
			if (gravityDot < 0)
			{
				// ignore drag if near a surface - maintain speed
				if (_nearSurface.GetTotalOverlaps() > 0)
				{
					return Vector3.Zero;
				}
				float drag = _gravityStrength * 1.5f;
				return Vector3.Down * drag * (-gravityDot) * delta;
			}
			else
			{
				return Vector3.Down * _gravityStrength * gravityDot * delta;
			}
		}

		private void _CheckDebugSpawns()
		{
			if (Input.IsActionJustPressed("slot_5"))
			{
				Vector3 pos = _aimLaser.GetSpawnPosition();
				_game.CreateMob(pos, MobType.BattleshipA);
				//SpawnVolume vol = _game.CreateSpawnVolume(pos);
				//vol.mobType = MobType.Cross;
			}
			if (Input.IsActionPressed("slot_0"))
			{
				_game.SpawnQuickPickups(_aimLaser.GetSpawnPosition(), 1);
			}
		}

		private void _TickDebugInputs(PlayerInput input, float delta)
		{
			/*if (Input.IsActionJustPressed("attack_2"))
			{
				var info = ProjectileLaunchInfo.FromNode3D(this, 10, Interactions.TEAM_ID_PLAYER, 1);
				IProjectile prj = _game.CreateProjectile(2);
				prj.Launch(info);

				info.t = info.t.RotateAtOrigin(info.t.basis.z, 90f * ZGU.DEG2RAD);
				prj = _game.CreateProjectile(2);
				prj.Launch(info);
			}*/
		}

		public void Reset()
		{
			GlobalPosition = _origin;
			Velocity = Vector3.Zero;
			_angularVelocity = Vector3.Zero;
		}

		private void _SpinFormationNodes(float delta)
		{
			float spinRadians = 360f * ZGU.DEG2RAD;
			_gatlingNode.RotateZ(spinRadians * (float)delta);
			_boostNode.RotateZ(spinRadians * (float)delta);
			_lockOnLeftNode.RotateZ(spinRadians * delta);
			_lockOnRightNode.RotateZ(-spinRadians * delta);
		}

		public override void _Process(double delta)
		{
			_SpinFormationNodes((float)delta);
			//_bodyMeshes.Visible = (_formation != TurretFormation.Narrow && _cameraPylon.GetLastFraction() > 0.25f);
			_bodyMeshes.Visible = _cameraPylon.GetLastFraction() > 0.25f;
		}

		private void BroadcastHudStatus()
		{
			_hudStatus.health = _health;
			GlobalEvents.Send(GameEvents.HUD_STATUS, _hudStatus);
		}

		public override void _PhysicsProcess(double delta)
		{
			BroadcastHudStatus();

			if (_game.IsMouseLocked())
			{
				return;
			}
			if (Input.IsActionJustPressed("reset"))
			{
				Reset();
			}

			if (Input.IsActionJustPressed("attack_4"))
			{
				_targetLocked = !_targetLocked;
			}
			if (!_targetLocked)
			{
				_aimPosition = _aimLaser.GetAimPosition();
			}

			_CheckDebugSpawns();

			PlayerInput input = _game.IsMouseLocked()
				? PlayerInput.Empty
				: PlayerUtils.GetInput();

			_TickDebugInputs(input, (float)delta);

			// attack
			if (input.slot1)
			{
				SetFormation(TurretFormation.Spread);
			}
			if (input.slot2)
			{
				SetFormation(TurretFormation.LockOn);
			}
			if (input.slot3)
			{
				SetFormation(TurretFormation.Narrow);
			}
			if (input.slot4)
			{
				SetFormation(TurretFormation.Boost);
			}

			// movement
			if (input.boosting)
			{
				boostGauge += boostPerSecond * (float)delta;

			}
			else if (boostGauge > 0)
			{
				// apply boost!
				if (boostGauge > 100)
				{
					boostGauge = 100;
				}
				float boostSpeed = 50f * (boostGauge / 100f);
				Vector3 boostForward = -this.GlobalTransform.basis.z;
				Velocity = boostForward * boostSpeed;
				boostGauge = 0f;
			}

			// roll
			float rollrate = 75f / 12f;
			float rollDegrees = input.pitchYawRoll.z * (rollrate * (float)delta);
			float pitchDegrees = input.pitchYawRoll.x * (90f * (float)delta);
			float yawDegrees = input.pitchYawRoll.y * (90f * (float)delta);
			if (Mathf.Abs(_angularVelocity.z) < 0.1)
			{
				_angularVelocity.z = 0;
			}
			if (rollDegrees != 0)
			{
				_angularVelocity.z += rollDegrees;
			}
			else
			{
				_angularVelocity.z *= 0.95f;
			}
			_angularVelocity.z = Mathf.Clamp(_angularVelocity.z, -10, 10);
			this.Rotate(GlobalTransform.basis.z.Normalized(), _angularVelocity.z * PlayerUtils.DEG2RAD);

			if (pitchDegrees != 0)
			{
				this.Rotate(GlobalTransform.basis.x.Normalized(), pitchDegrees * PlayerUtils.DEG2RAD);
			}
			if (yawDegrees != 0)
			{
				this.Rotate(GlobalTransform.basis.y.Normalized(), yawDegrees * PlayerUtils.DEG2RAD);
			}

			// push
			Vector3 push = PlayerUtils.InputAxesToCharacterPush(input.pushAxes, this.GlobalTransform.basis);

			float cappedSpeed = _moveSettings.maxSpeed;
			float currentSpeed = Velocity.Length();
			if (currentSpeed > cappedSpeed)
			{
				cappedSpeed = currentSpeed;
			}
			
			if (push.LengthSquared() > 0)
			{
				// apply thrust
				Vector3 thrust = push * (_moveSettings.pushForce * (float)delta);
				Velocity += thrust;
				// don't allow the push to exceed the cap
				if (Velocity.LengthSquared() > (cappedSpeed * cappedSpeed))
				{
					Velocity = Velocity.Normalized() * cappedSpeed;
				}

				// meh no gravity for now
				// Velocity += _CalcGravity(Velocity.Normalized(), (float)delta);
			}
			else
			{
				// apply drag
				Velocity *= _moveSettings.drag;
			}

			if (Velocity.LengthSquared() < (0.05f * 0.05f))
			{
				Velocity = Vector3.Zero;
			}

			MoveAndSlide();
			_RefreshDebugText();

		}

		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventMouseMotion)
			{
				if (_game.IsMouseLocked()) { return; }
				Transform3D t = GlobalTransform;
				InputEventMouseMotion motion = @event as InputEventMouseMotion;
				float yawRadians = (-motion.Relative.x) * 0.003f;
				this.Rotate(t.basis.y.Normalized(), yawRadians);
				float pitchRadians = (motion.Relative.y) * 0.003f;
				this.Rotate(t.basis.x.Normalized(), pitchRadians);
			}
			base._Input(@event);
		}
	}
}
