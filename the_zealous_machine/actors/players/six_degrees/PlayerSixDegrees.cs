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
		private RaycastPylon _cameraPylon;

		private List<PlayerTurret> _turrets = new List<PlayerTurret>();
		private List<Node3D> _spreadTurretPositions = new List<Node3D>();
		private List<Node3D> _narrowTurretPositions = new List<Node3D>();

		public override void _Ready()
		{
			_cameraPylon = GetNode<RaycastPylon>("head/raycast_pylon");
			_bodyMeshes = GetNode<Node3D>("body_meshes");
			_nearSurface = GetNode<Area3D>("near_surface");
			_debugText = GetNode<Label>("debug_print");
			Servicelocator.Get().GetService<MouseLock>().RemoveLock("player");
			_origin = GlobalTransform.origin;

			_gatlingNode = GetNode<Node3D>("head/options/gatling_node");


			_turrets.Add(GetNode<PlayerTurret>("head/options/player_turret"));
			_turrets.Add(GetNode<PlayerTurret>("head/options/player_turret2"));
			_turrets.Add(GetNode<PlayerTurret>("head/options/player_turret3"));
			_turrets.Add(GetNode<PlayerTurret>("head/options/player_turret4"));
			_narrowTurretPositions.Add(GetNode<Node3D>("head/options/gatling_node/target"));
			_narrowTurretPositions.Add(GetNode<Node3D>("head/options/gatling_node/target2"));
			_narrowTurretPositions.Add(GetNode<Node3D>("head/options/gatling_node/target3"));
			_narrowTurretPositions.Add(GetNode<Node3D>("head/options/gatling_node/target4"));
			_spreadTurretPositions.Add(GetNode<Node3D>("head/options/raycast_pylon/target"));
			_spreadTurretPositions.Add(GetNode<Node3D>("head/options/raycast_pylon2/target"));
			_spreadTurretPositions.Add(GetNode<Node3D>("head/options/raycast_pylon3/target"));
			_spreadTurretPositions.Add(GetNode<Node3D>("head/options/raycast_pylon4/target"));
			SetToWideFormation();

			// recursive search doesn't seem to work... not sure what I'm doing wrong?
			//Godot.Collections.Array<Node> turrets = FindChildren("player_turret", "", true, false);
			//List<PlayerTurret> turrets = FindChildren("*player_turret*", "", true, false).Select(x => x as PlayerTurret).ToList();
			GD.Print($"Player found {_turrets.Count} turrets");

			this.TreeExiting += _OnTreeExiting;
			_game = Servicelocator.Locate<IGame>();
			_game.RegisterPlayer(this);
		}

		private void _OnTreeExiting()
		{
			_game.UnregisterPlayer(this);
		}

		private void SetToNarrowFormation()
		{
			_turrets[0].SetTrackTarget(_narrowTurretPositions[0]);
			_turrets[1].SetTrackTarget(_narrowTurretPositions[1]);
			_turrets[2].SetTrackTarget(_narrowTurretPositions[2]);
			_turrets[3].SetTrackTarget(_narrowTurretPositions[3]);
		}

		private void SetToWideFormation()
		{
			_turrets[0].SetTrackTarget(_spreadTurretPositions[0]);
			_turrets[1].SetTrackTarget(_spreadTurretPositions[1]);
			_turrets[2].SetTrackTarget(_spreadTurretPositions[2]);
			_turrets[3].SetTrackTarget(_spreadTurretPositions[3]);
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
			_debugStr.Append($"Boost: {boostGauge}\n");
			_debugStr.Append($"Speed: {Velocity.Length()}\nVelocity: {Velocity}");
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

		public override void _Process(double delta)
		{
			_bodyMeshes.Visible = (_cameraPylon.GetLastFraction() > 0.25f);
			_gatlingNode.RotateZ((180f * ZGU.DEG2RAD) * (float)delta);

		}

		public override void _PhysicsProcess(double delta)
		{
			if (Input.IsActionJustPressed("reset"))
			{
				GlobalPosition = _origin;
				Velocity = Vector3.Zero;
				_angularVelocity = Vector3.Zero;
			}

			PlayerInput input = PlayerUtils.GetInput();

			// attack
			if (input.slot1)
			{
				SetToWideFormation();
			}
			if (input.slot2)
			{
				SetToNarrowFormation();
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
			_angularVelocity.z = Mathf.Clamp(_angularVelocity.z, -180, 180);
			this.Rotate(GlobalTransform.basis.z.Normalized(), _angularVelocity.z * PlayerUtils.DEG2RAD);

			// push
			Vector3 push = PlayerUtils.InputAxesToCharacterPush(input.pushAxes, this.GlobalTransform.basis);

			float cappedSpeed = 20f;
			float currentSpeed = Velocity.Length();
			if (currentSpeed > cappedSpeed)
			{
				cappedSpeed = currentSpeed;
			}
			
			if (push.LengthSquared() > 0)
			{
				// apply thrust
				Vector3 thrust = push * (160f * (float)delta);
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
				Velocity *= 0.97f;
			}

			MoveAndSlide();
			_RefreshDebugText();

		}

		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventMouseMotion)
			{
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
