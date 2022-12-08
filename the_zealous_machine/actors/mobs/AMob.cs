using Godot;
using System;
using TheZealousMachine.actors.projectiles;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.mobs
{
	public partial class AMob : CharacterBody3D, IHittable, IMob
	{
		protected Guid _id = Guid.Empty;
		protected IGame _game;
		protected MobThinkInfo _think = new MobThinkInfo();
		protected Node3D _head;
		protected MobSensors _sensors;
		protected Vector3 _spawnOrigin;

		bool _dead = false;
		protected int _health = 80;
		protected float _shootTick = 1;
		protected float _shootTime = 0.5f;
		protected ProjectileType _nextProjectileType = ProjectileType.MobBasic;
		protected bool _onlyMoveIfOutOfLoS = false;

		public override void _Ready()
		{
			_game = Servicelocator.Locate<IGame>();
			_head = GetNode<Node3D>("head");
			_sensors = GetNode<MobSensors>("mob_sensors");
			_game.RegisterMob(this);
			GlobalEvents.Send(GameEvents.MOB_SPAWNED, this);
		}

		public void Teleport(Vector3 pos)
		{
			GlobalPosition = pos;
			_spawnOrigin = pos;
        }


        public Node3D GetBaseNode() { return this; }


		public override void _ExitTree()
		{
			_game.UnregisterMob(this);
		}

		public void SetMobId(Guid id)
		{
			_id = id;
		}

		public Guid mobId { get { return _id; } }

		virtual protected void _RemoveFromGame()
		{
			if (_dead) { return; }
			_dead = true;
			_game.SpawnQuickPickups(GlobalPosition, 3);
			_game.CreateMobDebris(GlobalPosition, Velocity.Normalized());
			//GD.Print($"{Name} - killed");
			GlobalEvents.Send(GameEvents.MOB_DIED, this);
			QueueFree();
		}

		virtual public HitResponse Hit(HitInfo hit)
		{
			HitResponse response = HitResponse.Empty;
			if (_dead)
			{
				return response;
			}
			_health -= hit.damage;
			response.type = HitResponseType.Damaged;
			response.damage = hit.damage;
			if (_health < 0)
			{
				_RemoveFromGame();
			}
			else
			{
				Velocity += (hit.direction * 5f);
			}
			return response;
		}

		protected void _LookInDirectionOfMovement()
		{
			Vector3 vel = Velocity;
			if (vel.LengthSquared() == 0) { return; }
			Vector3 pos = GlobalPosition;
			this.LookAtSafe(pos + vel, Vector3.Up, Vector3.Left);
		}

		protected void _LookAtThinkTarget()
		{
			if (_think.targetInfo.valid == false)
			{
				return;
			}
			this.LookAtSafe(_think.targetInfo.position, Vector3.Up, Vector3.Left);
		}

		virtual protected ProjectileLaunchInfo CreateLaunchInfo(ProjectileType prjType)
		{
			ProjectileLaunchInfo info = new ProjectileLaunchInfo();
			info.damage = 10;
			info.speed = 30f;
			return info;
		}

		virtual protected void _Shoot()
		{
			Transform3D t = GlobalTransform;
			ProjectileLaunchInfo info = CreateLaunchInfo(_nextProjectileType);
			info.t = t.LookingAt(_think.targetInfo.position, Vector3.Up);
			info.teamId = Interactions.TEAM_ID_MOBS;

			IProjectile prj = _game.CreateProjectile(_nextProjectileType);
			prj.Launch(info);
		}

		virtual protected void _PushMoveToward(Vector3 target, float pushForce, float maxSpeed, float delta)
		{
			Vector3 velocity = Velocity;
			velocity += (_think.toward * pushForce) * delta;
			velocity = velocity.ClampMagnitude(maxSpeed);
			Velocity = velocity;
			MoveAndSlide();
		}

		virtual protected void _HuntingTick(float delta)
		{
			if (_onlyMoveIfOutOfLoS && !_think.canSeeTarget)
			{
				_PushMoveToward(_think.toward, 20f, 20f, delta);
                _LookInDirectionOfMovement();
            }
			else
			{
                _LookAtThinkTarget();
            }

			_shootTick -= (float)delta;
			if (_shootTick <= 0)
			{
				_shootTick = _shootTime;
				_Shoot();
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			_think.Refresh(this, _game);
			if (!_think.HasTarget())
			{
				return;
			}
			if (_sensors.overlaps == 0)
			{
				GD.Print($"Mob out of bounds - teleporting");
				Teleport(_spawnOrigin);
			}

			_HuntingTick((float)delta);
		}
	}
}
