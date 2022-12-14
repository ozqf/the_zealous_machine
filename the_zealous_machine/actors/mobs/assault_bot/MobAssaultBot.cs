
using Godot;

namespace TheZealousMachine.actors.mobs.assault_bot
{
	public partial class MobAssaultBot : AMob
	{
		private Node3D _launchLeft;
		private Node3D _launchRight;

		private Vector3 _movePoint = new Vector3();
		private float _moveThinkTick = 0f;
		private float _lastMoveThinkTick = 2f;
		private float _evadeSpeed = 30f;
		private Vector3 _evadeDir = new Vector3();


		public override void _Ready()
		{
			base._Ready();
			_health = 1500;
			_shootTime = 0.2f;
			_stunThreshold = 1000;
			//_onlyMoveIfOutOfLoS = true;
			_launchLeft = GetNode<Node3D>("gun_left");
			_launchRight = GetNode<Node3D>("gun_right");
		}

		private void _SetMoveThinkTick(float moveThinkTick)
		{
			_moveThinkTick = moveThinkTick;
			_lastMoveThinkTick = _moveThinkTick;
		}

		protected void _SwoopMove(float delta)
		{
			_moveThinkTick -= delta;
			if (_moveThinkTick <= 0f)
			{
				_moveThinkTick = 0.5f;
				float proximityCap = 50f;
				if (_think.distSqr > proximityCap * proximityCap)
				{
					_movePoint = _think.targetInfo.position;
				}
				else
				{
					_moveThinkTick = 2f;
					Transform3D t = GlobalTransform;
					t = t.LookingAt(_think.targetInfo.position, Vector3.Up);
					Vector3 left = t.basis.x;
					//Vector3 up = Vector3.Zero;
					Vector3 up = t.basis.y;
					left *= (float)GD.RandRange(-1f, 1f);
					up *= (float)GD.RandRange(-1f, 1f);
					Vector3 dir = (left + up).Normalized();
					GD.Print($"dir {dir}");
					//_think.targetInfo.
					_movePoint = _think.targetInfo.position + (dir * 100f);
				}
			}
			_PushMoveTowardPoint(_movePoint, 40f, 25f, delta);
			_LookInDirectionOfMovement();
		}

		protected void _EvadeMove(float delta)
		{
			_moveThinkTick -= delta;
			if (_moveThinkTick <= 0f)
			{
				_moveThinkTick = 2f;
				Transform3D t = GlobalTransform;
				t = t.LookingAt(_think.targetInfo.position, Vector3.Up);
				Vector3 left = t.basis.x;
				//Vector3 up = Vector3.Zero;
				Vector3 up = t.basis.y;
				left *= (float)GD.RandRange(-1f, 1f);
				up *= (float)GD.RandRange(-1f, 1f);
				_evadeDir = (left + up).Normalized();

				Velocity = _evadeDir * 30f;
			}
			else
			{
				float weight = _moveThinkTick / (_lastMoveThinkTick + 1);
				weight = 1 - weight;
				float speed = Mathf.Lerp(_evadeSpeed, 0f, weight);
				Velocity = Velocity.Normalized() * speed;
			}
			MoveAndSlide();
			//_LookInDirectionOfMovement();
			_LookAtThinkTarget();
		}

		protected override void _HuntingTick(float delta)
		{
			_EvadeMove(delta);

		}

	}
}
