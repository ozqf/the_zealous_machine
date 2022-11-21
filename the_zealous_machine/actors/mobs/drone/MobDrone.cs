using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.mobs
{
	public partial class MobDrone : CharacterBody3D, IHittable
	{
		private IGame _game;
		private MobThinkInfo _think = new MobThinkInfo();
		private Node3D _head;

		private bool _dead = false;
		private int _health = 80;

		public override void _Ready()
		{
			_game = Servicelocator.Locate<IGame>();
			_head = GetNode<Node3D>("head");
		}

		private void _RemoveFromGame()
		{
			_dead = true;
			_game.CreateMobDebris(GlobalPosition, Velocity.Normalized());
			GD.Print($"{Name} - killed");
			GlobalEvents.Send(GameEvents.MOB_DIED, this);
			QueueFree();
		}

		public HitResponse Hit(HitInfo hit)
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

		private void _LookInDirectionOfMovement()
		{
			Vector3 vel = Velocity;
			if (vel.LengthSquared() == 0) { return; }
			Vector3 pos = GlobalPosition;
			this.LookAtSafe(pos + vel, Vector3.Up, Vector3.Left);

		}

		public override void _PhysicsProcess(double delta)
		{
			_think.Refresh(this, _game);
			if (!_think.HasTarget())
			{
				return;
			}

			Vector3 velocity = Velocity;
			velocity += (_think.toward * 20f) * (float)delta;
			velocity = velocity.ClampMagnitude(20f);
			Velocity = velocity;
			MoveAndSlide();
			_LookInDirectionOfMovement();

		}
	}
}
