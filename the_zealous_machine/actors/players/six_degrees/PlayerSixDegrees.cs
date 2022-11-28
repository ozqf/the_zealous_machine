using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheZealousMachine.actors.players;
using ZealousGodotUtils;

namespace TheZealousMachine
{
	public partial class PlayerSixDegrees : CharacterBody3D, IPlayer
	{
		private IGame _game;

		Vector3 _origin;
		Vector3 _angularVelocity = Vector3.Zero;
		float _gravityStrength = 20f;
		private float boostGauge = 0f;
		private float boostPerSecond = 50f;
		private Label _debugText;
		private StringBuilder _debugStr = new StringBuilder();
		private Area3D _nearSurface;
		private Node3D _bodyMeshes;
		private Node3D _gatlingNode;
		private Node3D _boostNode;
		private RaycastPylon _cameraPylon;
		private AimLaser _aimLaser;

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
			_nearSurface = GetNode<Area3D>("near_surface");
			_debugText = GetNode<Label>("debug_print");
			_origin = GlobalTransform.origin;

			_gatlingNode = GetNode<Node3D>("head/options/gatling_node");
			_boostNode = GetNode<Node3D>("head/options/boost_node");
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

			int turretCount = _remoteTurrets.Count;
			for (int i = 0; i < turretCount; i++)
			{
				_remoteTurrets[i].SetTrackTargets(_spreadTurretPositions[i], _narrowTurretPositions[i], _boostTurretPositions[i]);
				_remoteTurrets[i].SetAimTarget(_aimLaser);
			}

			_centreTurret.SetAimTarget(_aimLaser);

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
			if (_formation == TurretFormation.Narrow)
			{
				_cameraPylon.OverrideFarPositionTarget(GetNode<Node3D>("head/raycast_pylon/close"));
			}
			else
			{
				_cameraPylon.OverrideFarPositionTarget(GetNode<Node3D>("head/raycast_pylon/far"));
			}
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
			_debugStr.Append($"Formation: {_formation}");
			_debugStr.Append($"Push max {GetMaxPushSpeed()} push force {GetPushForce()}\n");
			_debugStr.Append($"Boost: {boostGauge}\n");
			_debugStr.Append($"Speed: {Velocity.Length()}\nVelocity: {Velocity}\n");
			_debugStr.Append($"Angular Velocity: {_angularVelocity}\n");
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
				_game.CreateSpawnVolume(pos);

			}
		}

		private float GetMaxPushSpeed()
		{
			switch (_formation)
			{
				case TurretFormation.Boost:
					return 30f;
				case TurretFormation.Narrow:
					return 10f;
				default:
					return 20f;
			}
		}

		public float GetPushForce()
		{
			switch (_formation)
			{
				case TurretFormation.Boost:
					return 100f;
				case TurretFormation.Narrow:
					return 200f;
				default:
					return 160f;
			}
		}
		
		public float GetDrag()
		{
			switch (_formation)
			{
				case TurretFormation.Boost:
					return 0.97f;
				case TurretFormation.Narrow:
					return 0.8f;
				default:
					return 0.97f;
			}
		}

		private void _TickDebugInputs(PlayerInput input, float delta)
		{
			if (Input.IsActionJustPressed("attack_2"))
			{
				var info = ProjectileLaunchInfo.FromNode3D(this, 10, Interactions.TEAM_ID_PLAYER, 1);
				IProjectile prj = _game.CreateProjectile(2);
				prj.Launch(info);

				//Vector3 origin = info.t.origin;
				//info.t.origin = Vector3.Zero;
				//info.t = info.t.Rotated(info.t.basis.z, 90f * ZGU.DEG2RAD);
				//info.t.origin = origin;
				info.t = info.t.RotateAtOrigin(info.t.basis.z, 90f * ZGU.DEG2RAD);
				prj = _game.CreateProjectile(2);
				prj.Launch(info);
			}
		}

		public void Reset()
		{
			GlobalPosition = _origin;
			Velocity = Vector3.Zero;
			_angularVelocity = Vector3.Zero;
		}

		public override void _Process(double delta)
		{
			float spinRadians = 360f * ZGU.DEG2RAD;
			_bodyMeshes.Visible = (_formation != TurretFormation.Narrow && _cameraPylon.GetLastFraction() > 0.25f);
			_gatlingNode.RotateZ(spinRadians * (float)delta);
			_boostNode.RotateZ(spinRadians * (float)delta);
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_game.IsMouseLocked())
			{
				return;
			}
			if (Input.IsActionJustPressed("reset"))
			{
				Reset();
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
				SetFormation(TurretFormation.Narrow);
			}
			if (input.slot3)
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
			float rollDegrees = input.roll.z * (rollrate * (float)delta);
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

			// push
			Vector3 push = PlayerUtils.InputAxesToCharacterPush(input.pushAxes, this.GlobalTransform.basis);

			float cappedSpeed = GetMaxPushSpeed();
			float currentSpeed = Velocity.Length();
			if (currentSpeed > cappedSpeed)
			{
				cappedSpeed = currentSpeed;
			}
			
			if (push.LengthSquared() > 0)
			{
				// apply thrust
				Vector3 thrust = push * (GetPushForce() * (float)delta);
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
				Velocity *= GetDrag();
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
