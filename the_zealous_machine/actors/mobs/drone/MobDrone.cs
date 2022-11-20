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
		private int _health = 100;

		public override void _Ready()
		{
			_game = Servicelocator.Locate<IGame>();
			_head = GetNode<Node3D>("head");
		}

		public HitResponse Hit(HitInfo hit)
		{
			HitResponse response = HitResponse.Empty;
			if (_dead)
			{
				return response;
			}
			_health -= hit.damage;
			response.type = HitResposneType.Damaged;
			response.damage = hit.damage;
			if (_health < 0)
			{
				_dead = true;
				GD.Print($"{Name} - killed");
				QueueFree();
			}
			return response;
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
		}
	}
}
