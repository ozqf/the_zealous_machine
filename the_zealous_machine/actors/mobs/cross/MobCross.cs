using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.mobs.cross
{
	public partial class MobCross : AMob
	{
		public override void _Ready()
		{
			base._Ready();
			_nextProjectileType = ProjectileType.Column;
			_shootTime = 2f;
			_health = 200;
			_onlyMoveIfOutOfLoS = true;
		}

		protected override void _PushMoveToward(Vector3 target, float pushForce, float maxSpeed, float delta)
		{
			pushForce = 10f;
			maxSpeed = 10f;
			base._PushMoveToward(target, pushForce, maxSpeed, delta);
		}

		protected override ProjectileLaunchInfo CreateLaunchInfo(ProjectileType prjType)
		{
			ProjectileLaunchInfo info = base.CreateLaunchInfo(prjType);
			info.speed = 10f;
			return info;
		}

		override protected void _Shoot()
		{
			Transform3D t = GlobalTransform;
			ProjectileLaunchInfo info = CreateLaunchInfo(_nextProjectileType);
			info.t = t.LookingAt(_think.targetInfo.position, Vector3.Up);
			info.teamId = Interactions.TEAM_ID_MOBS;

			IProjectile prj = _game.CreateProjectile(_nextProjectileType);
			prj.Launch(info);

			info.t = info.t.RotateAtOrigin(info.t.basis.z, 90f * ZGU.DEG2RAD);
			prj = _game.CreateProjectile(_nextProjectileType);
			prj.Launch(info);
		}
	}
}
