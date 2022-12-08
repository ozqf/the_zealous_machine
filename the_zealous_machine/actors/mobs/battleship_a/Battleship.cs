using Godot;
using System;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.mobs.battleship_a
{
	public partial class Battleship : CharacterBody3D, IMob, IHittable
	{
		private Guid _mobId;
		private int _health = 1000;

		public Guid mobId => _mobId;
		private bool _dead = false;

		public override void _Ready()
		{
			GlobalEvents.Send(GameEvents.MOB_SPAWNED, this);
		}

		public void Teleport(Vector3 pos)
		{
			GlobalPosition = pos;
		}

		public Node3D GetBaseNode()
		{
			return this;
		}

		private void _Remove()
		{
			if (_dead) { return; }
			_dead = true;
			QueueFree();
			GlobalEvents.Send(GameEvents.MOB_DIED, this);
		}

		public HitResponse Hit(HitInfo hit)
		{
			if (_health <= 0)
			{
				return HitResponse.Empty;
			}
			_health -= hit.damage;
			if (_health <= 0)
			{
				_Remove();
			}
			return new HitResponse
			{
				type = HitResponseType.Damaged,
				damage = hit.damage
			};
		}

		public void SetMobId(Guid id)
		{
			_mobId = id;
		}
	}
}
