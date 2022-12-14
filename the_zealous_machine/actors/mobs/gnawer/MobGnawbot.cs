using TheZealousMachine.actors.mobs.shark;

namespace TheZealousMachine.actors.mobs.gnawer
{
	public partial class MobGnawbot : MobShark
	{
		public override void _Ready()
		{
			base._Ready();
			_health = 300;
		}

		protected override void _ReactToHit(HitInfo hit)
		{
			
		}

		override protected void _HuntingTick(float delta)
		{
			if (_think.canSeeTarget)
			{
				// chaaaaaaarge
				_AccelerateTowardsPoint(_think.targetInfo.position, delta, 40f, 0.995f);
				_LookInDirectionOfMovement();
			}
			else
			{
				// creep toward player
				_PushMoveByDirection(_think.toward, 25f, 15f, delta);
				_LookInDirectionOfMovement();
			}
		}

	}
}
