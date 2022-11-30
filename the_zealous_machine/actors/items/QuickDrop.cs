using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.items
{
	public partial class QuickDrop : RigidBody3D
	{
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

		private void _PushToPoint(Vector3 p, float delta)
		{
			Vector3 toward = (p - GlobalPosition).Normalized();
			if (LinearVelocity.Dot(toward) < 0)
			{
				LinearVelocity *= 0.95f;
			}
			LinearVelocity += (toward * 120f) * delta;
		}

		private void _Remove()
		{
			_tick = 9999999;
			QueueFree();
			SetProcess(false);
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
			float collectRange = 12f;
			if (dist > (collectRange * collectRange) && !_grabbed)
			{
				return;
			}
			else if (dist > 1f)
			{
				if (collector.CanTake("energy", 1) > 0)
				{
					_grabbed = true;
					_PushToPoint(collector.GlobalPosition, (float)delta);
				}
				else
				{
					_grabbed = false;
				}
			}
			else
			{
				if (collector.GiveItem("energy", 1) > 0)
				{
					_Remove();
				}
			}
		}
	}
}
