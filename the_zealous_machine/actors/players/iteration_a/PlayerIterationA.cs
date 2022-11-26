using Godot;
using System;
using System.Text;
using ZealousGodotUtils;

namespace TheZealousMachine
{
	public partial class PlayerIterationA : CharacterBody3D, IPlayer
	{
		private Node3D _head;
		private Vector3 _origin = new Vector3();
		private RayCast3D _groundRay;
		private float boostGauge = 0f;
		private float boostPerSecond = 50f;
		private Label _debugText;
		private PlayerDebugInfo _debugInfo = new PlayerDebugInfo();
		private StringBuilder _debugStr = new StringBuilder();

		public override void _Ready()
		{
			_head = GetNode<Node3D>("head");
			_groundRay = GetNode<RayCast3D>("ground_ray");
			_debugText = GetNode<Label>("debug_print");
            Servicelocator.Get().GetService<MouseLock>().RemoveLock("player");
			_origin = GlobalTransform.origin;
		}

		private void _UpdateDebutText()
		{
			_debugStr.Clear();
			_debugStr.Append($"Boost: {boostGauge}\n");
			_debugStr.Append($"Speed: {(int)this.Velocity.Length()}\n");
			_debugStr.Append($"Hit pos: {_debugInfo.lastMoveContactPosition}\n");
			_debugStr.Append($"Hit normal: {_debugInfo.lastMoveContactNormal}\n");
			_debugText.Text = _debugStr.ToString();
		}

        public void Reset()
        {
            GlobalPosition = _origin;
            Velocity = Vector3.Zero;
        }

        private void _TickMovement(double delta, PlayerInput input)
		{
			if (_groundRay.IsColliding() && this.IsOnFloor())
			{
				this.UpDirection = _groundRay.GetCollisionNormal();
			}
			else
			{
				this.UpDirection = new Vector3(0, 1, 0);
			}

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
				float boostSpeed = 40f * (boostGauge / 100f);
				Vector3 boostForward = -_head.GlobalTransform.basis.z;
				Velocity = boostForward * boostSpeed;
				boostGauge = 0f;
			}

			Transform3D bodyT = this.GlobalTransform;
			if (Input.IsActionJustPressed("reset"))
			{
				bodyT.origin = _origin;
				this.GlobalTransform = bodyT;
				Velocity = new Vector3();
			}
			//Basis pushBasis = _head.GlobalTransform.basis;
			//Basis pushBasis = bodyT.basis;
			//Vector3 input = GetInputAxes();
			Vector3 push = PlayerUtils.InputAxesToCharacterPush(input.pushAxes, bodyT.basis);
			//Vector3 push = new Vector3();
			//push += (-pushBasis.z) * input.pushAxes.z;
			//push += pushBasis.y * input.pushAxes.y;
			//push += (pushBasis.x) * input.pushAxes.x;

			Vector3 newFlatVelocity = Velocity;
			//newFlatVelocity.y = 0;
			newFlatVelocity.x += push.x * 2f;
			newFlatVelocity.z += push.z * 2f;
			newFlatVelocity.y += push.y * 2f;

			Vector3 currentFlatVelocity = Velocity;
			currentFlatVelocity.y = 0;
			float velocityCap = 20f;
			float curFlatSpeed = currentFlatVelocity.Length();
			if (curFlatSpeed > velocityCap)
			{
				velocityCap = curFlatSpeed;
			}

			if (newFlatVelocity.Length() > velocityCap)
			{
				newFlatVelocity = newFlatVelocity.Normalized() * velocityCap;

			}
			Vector3 newVelocity = Velocity;
			newVelocity.x = newFlatVelocity.x;
			newVelocity.y = newFlatVelocity.y;
			newVelocity.z = newFlatVelocity.z;
			//Velocity.y = currentY;
			// gravity
			//newVelocity += new Vector3(0, -20f, 0) * (float)delta;

			Velocity = newVelocity;
			float speed = Velocity.Length();
			bool hitSomething = this.MoveAndSlide();

			// maintain speed on glancing impacts
			Vector3 postMoveVelocity = Velocity;
			if (postMoveVelocity.Dot(newVelocity) > 0.5f)
			{
				Velocity = postMoveVelocity.Normalized() * speed;
			}
			/*else if (speed > 0.1)
			{
				GD.Print($"Sudden stop!");
			}
			*/
			// TODO: Maybe this can be scanned to get info on potential
			// ghost collisions?
			KinematicCollision3D info = this.GetLastSlideCollision();
			if (info != null)
			{
				_debugInfo.lastMoveContactPosition = info.GetPosition();
				_debugInfo.lastMoveContactNormal = info.GetNormal();
			}
			else
			{
				_debugInfo.lastMoveContactPosition = Vector3.Zero;
				_debugInfo.lastMoveContactNormal = Vector3.Zero;
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			PlayerInput input = PlayerUtils.GetInput();
			_TickMovement(delta, input);
			_UpdateDebutText();
		}

		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventMouseMotion)
			{
				InputEventMouseMotion motion = @event as InputEventMouseMotion;
				float yaw = (-motion.Relative.x) * 0.005f;
				this.RotateY(yaw);
				float pitch = (motion.Relative.y) * 0.005f;
				_head.RotateX(pitch);
			}
			base._Input(@event);
		}

		public TargetInfo GetTargetInfo()
		{
			TargetInfo info = new TargetInfo();
			info.valid = true;
			info.position = this.GlobalTransform.origin;
			return info;
		}
	}
}
