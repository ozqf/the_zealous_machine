using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.items
{
	public partial class QuickDrop : RigidBody3D
	{
		[Export]
		public string itemType = "energy";
		[Export]
		public int amountToGive = 1;

		public float _tick = 30f;
		private Area3D _area;
		private IGame _game;
		private bool _grabbed = false;

		public override void _Ready()
		{
			_area = GetNode<Area3D>("Area3D");
			_area.AreaEntered += _OnAreaEntered;
			_game = Servicelocator.Locate<IGame>();
		}

		public void Launch(Vector3 p)
		{
			GlobalPosition = p;
			Vector3 dir = Vector3.Up + new Vector3(
				(float)GD.RandRange(-0.5f, 0.5f),
				(float)GD.RandRange(-0.5f, 0.5f),
				(float)GD.RandRange(-0.5f, 0.5f)
				);
			dir = dir.Normalized();
			LinearVelocity = dir * 10f;
		}

		private void _OnAreaEntered(Area3D area)
		{
			//IItemCollector collector = area as IItemCollector;
			//if (collector == null) { return; }
			//collector.GiveItem()
		}

		private void _PushToPoint(Vector3 p, float delta, float pushForce, float overshootDrag)
		{
			Vector3 toward = (p - GlobalPosition).Normalized();
			if (LinearVelocity.Dot(toward) < 0)
			{
				LinearVelocity *= overshootDrag;
			}
			LinearVelocity += (toward * pushForce) * delta;
		}

		private void _Remove()
		{
			_tick = 9999999;
			QueueFree();
			SetPhysicsProcess(false);
		}

		public override void _PhysicsProcess(double delta)
		{
			_tick -= (float)delta;
			if (_tick <= 0 )
			{
				_Remove();
				return;
			}

			IItemCollector collector = _game.GetPlayerCollector();
			if (collector == null) { return; }
			
			float dist = collector.GlobalPosition.DistanceSquaredTo(GlobalPosition);
			float collectRange = 20f;
			if (dist > (collectRange * collectRange) && !_grabbed)
			{
				return;
			}
			else if (dist > 1f)
			{
				if (collector.CanTake(itemType, amountToGive) > 0)
				{
					_grabbed = true;
					_PushToPoint(collector.GlobalPosition, (float)delta, 120f, 0.8f);
				}
				else
				{
					_grabbed = false;
				}
			}
			else
			{
				if (collector.GiveItem(itemType, amountToGive) > 0)
				{
					_Remove();
				}
			}
		}
	}
}
