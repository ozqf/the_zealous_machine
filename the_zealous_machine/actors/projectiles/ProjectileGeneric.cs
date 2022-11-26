using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.projectiles
{
	public partial class ProjectileGeneric : Node3D, IProjectile
	{
		private IGame _game;
		private float _speed = 50f;
		private RayCast3D _ray;
		private float _timeToLive = 30f;
		private int _teamId = Interactions.TEAM_ID_MOBS;
		private ImpactType _damagingImpactType = ImpactType.Yellow;
		private ImpactType _dudImpactType = ImpactType.Grey;
		private ProjectileLaunchInfo _launchInfo;

		public override void _Ready()
		{
			_game = Servicelocator.Locate<IGame>();
			_ray = GetNode<RayCast3D>("RayCast3D");
		}

		public Node3D GetPrjBaseNode() { return this; }

		public void Launch(ProjectileLaunchInfo launchInfo)
		{
			_launchInfo = launchInfo;
			Transform3D t = this.GlobalTransform;
			t.origin = launchInfo.position;
			Vector3 up = launchInfo.up;
			if (up == Vector3.Zero) { up = Vector3.Up; }
			t = t.LookingAt(launchInfo.position + launchInfo.forward, up);
			GlobalTransform = t;
			_speed = launchInfo.speed;
			_teamId = launchInfo.teamId;
			if (_teamId == Interactions.TEAM_ID_PLAYER)
			{
				_ray.CollisionMask = Interactions.GetPlayerProjectileMask();
				_damagingImpactType = ImpactType.Yellow;
				_dudImpactType = ImpactType.Grey;
			}
			else
			{
				_ray.CollisionMask = Interactions.GetEnemyProjectileMask();
				_damagingImpactType = ImpactType.Purple;
				_dudImpactType = ImpactType.Purple;
			}
		}

		private void _Step(float delta)
		{
			GlobalPosition += (-GlobalTransform.basis.z) * (_speed * delta);
		}

		public override void _PhysicsProcess(double delta)
		{
			float dt = (float)delta;
			_timeToLive -= dt;
			if ( _timeToLive <= 0 )
			{
				this.QueueFree();
				return;
			}
			float fraction = _ray.GetRayFraction();
			if (fraction == 1f)
			{
				// move
				_Step(dt);
				return;
			}
			float stepDist = _speed * dt;
			float weight = stepDist / _ray.TargetPosition.Length();
			if (fraction > weight)
			{
				_Step(dt);
				return;
			}
			// hit something
			IHittable victim = _ray.GetCollider() as IHittable;
			if (victim != null)
			{
				HitInfo hit = new HitInfo();
				hit.position = GlobalTransform.origin;
				hit.direction = -GlobalTransform.basis.z;
				hit.damage = _launchInfo.damage;
				HitResponse res = victim.Hit(hit);
				_game.CreateBulletImpact(_ray.GetCollisionPoint(), _ray.GetCollisionNormal(), _damagingImpactType);
				this.QueueFree();
			}
			else
			{
				_game.CreateBulletImpact(_ray.GetCollisionPoint(), _ray.GetCollisionNormal(), _dudImpactType);
				this.QueueFree();
			}
		}
	}
}
