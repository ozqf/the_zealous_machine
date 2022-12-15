using Godot;

namespace TheZealousMachine.actors.mobs.shark
{
	public partial class MobShark : AMob
	{
		private const float _maxSpeed = 18f;
		private Vector3 _strafeDir = Vector3.Forward;

		public override void _Ready()
		{
			base._Ready();
			_strafeDir.x = (float)GD.RandRange(-1f, 1f);
			_strafeDir.y = (float)GD.RandRange(-1f, 1f);
			_strafeDir.z = (float)GD.RandRange(-1f, 1f);
			_strafeDir = _strafeDir.Normalized();
			_shootTime = 0.3f;
		}

		protected override void _HuntingTick(float delta)
		{
			Vector3 movePoint = _think.targetInfo.t.origin + (_strafeDir * 15f);
			_PushMoveTowardPoint(movePoint, 50f, _maxSpeed, delta);
			_LookInDirectionOfMovement();

			_shootTick -= (float)delta;
			if (_shootTick <= 0)
			{
				_shootTick = _shootTime;
				_Shoot();
			}
		}
	}
}
