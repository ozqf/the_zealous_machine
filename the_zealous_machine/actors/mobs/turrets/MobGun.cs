using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.mobs.turrets
{
	public partial class MobGun : Node3D
	{
		[Export]
		public bool on = true;
		[Export]
		public ProjectileType projectileType = ProjectileType.MobBasic;
		[Export]
		public float projectileSpeed = 20;
		[Export]
		public float refireTime = 1f;
        [Export]
        public Vector3 degreesRandomRotation = new Vector3();

        private float _tick = 0f;
		private IGame _game;

		public override void _Ready()
		{
			_game = Servicelocator.Locate<IGame>();
		}

		virtual public void Shoot()
		{
			Transform3D t = GlobalTransform;
			if (degreesRandomRotation.z != 0)
			{
				float roll = degreesRandomRotation.z * ZGU.DEG2RAD;
				roll = (float)GD.RandRange(0, roll);
				//GD.Print($"Roll {roll}");
                t = t.RotateAtOrigin(t.basis.z, roll);
            }
            ProjectileLaunchInfo info = new()
			{
				damage = 10,
				speed = projectileSpeed,
				t = t,
				teamId = Interactions.TEAM_ID_MOBS
			};

			IProjectile prj = _game.CreateProjectile(projectileType);
			prj.Launch(info);
		}

		public override void _Process(double delta)
		{
			if (!on) { return; }
			_tick -= (float)delta;
			if (_tick <= 0f)
			{
				_tick = refireTime;
				Shoot();
			}
		}
	}
}
