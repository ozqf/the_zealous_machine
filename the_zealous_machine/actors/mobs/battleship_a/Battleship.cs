using Godot;
using System;

namespace TheZealousMachine.actors.mobs.battleship_a
{
	public partial class Battleship : CharacterBody3D, IMob, IHittable
	{
		private Guid _mobId;
		private int _health = 1000;

		public Guid mobId => _mobId;

		public Node3D GetBaseNode()
		{
			return this;
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
				QueueFree();
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
