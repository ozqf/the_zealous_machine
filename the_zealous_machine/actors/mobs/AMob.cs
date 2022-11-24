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

        bool _dead = false;
        protected int _health = 80;
        protected float _shootTick = 1;
        protected float _shootTime = 0.5f;

        public override void _Ready()
        {
            _game = Servicelocator.Locate<IGame>();
            _head = GetNode<Node3D>("head");
        }

        public void SetMobId(Guid id)
        {
            _id = id;
        }

        public Guid mobId { get { return _id; } }

        virtual protected void _RemoveFromGame()
        {
            _dead = true;
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

        virtual protected void _Shoot()
        {
            Transform3D t = GlobalTransform;
            ProjectileLaunchInfo info = new ProjectileLaunchInfo();
            Vector3 toward = _think.targetInfo.position - t.origin;
            toward = toward.Normalized();
            info.forward = toward;
            info.position = t.origin + (info.forward * 0.5f);
            info.speed = 40f;
            info.teamId = Interactions.TEAM_ID_MOBS;

            ProjectileGeneric prj = _game.CreateProjectile(1);
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
            _PushMoveToward(_think.toward, 20f, 20f, delta);
            _LookInDirectionOfMovement();

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

            _HuntingTick((float)delta);
        }
    }
}
