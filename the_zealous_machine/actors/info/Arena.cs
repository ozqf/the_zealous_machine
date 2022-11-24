using Godot;
using System.Collections.Generic;
using TheZealousMachine.actors.volumes;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.info
{
	public partial class Arena : Node3D, IArena
	{
		private bool _scanned = false;
		private List<Spawner> _spawners;
		private List<PlayerBarrier> _barriers;
		private List<IDoor> _doors;
		private List<RoomSeal> _seals;
		private bool _running = false;


		public override void _Ready()
		{
			ScanForComponents();
		}

		public void ScanForComponents()
		{
			if (_scanned) { return; }
			_scanned = true;
			_spawners = this.FindChildrenOfType<Spawner>();
			_barriers = this.FindChildrenOfType<PlayerBarrier>();
			_doors = this.FindChildrenOfType<IDoor>();
			_seals = this.FindChildrenOfType<RoomSeal>();
			GD.Print($"Arena {Name} found {_spawners.Count} spawners, {_seals.Count} seals, {_barriers.Count} barriers");
			_doors.ForEach(d => d.SetOpen(true));
		}

		public RoomSeal FindHighestSeal()
		{
			int numSeals = _seals.Count;
			if (numSeals == 0) { return null; }
			RoomSeal seal = _seals[0];
			float y = seal.Position.y;
			for (int i = 1; i < numSeals; i++)
			{
				RoomSeal query = _seals[i];
				if (query.Position.y > y)
				{
					y = query.Position.y;
					seal = query;
				}
			}
			return seal;
		}

		public RoomSeal FindLowestSeal()
		{
			int numSeals = _seals.Count;
			if (numSeals == 0) { return null; }
			RoomSeal seal = _seals[0];
			float y = seal.Position.y;
			for (int i = 1; i < numSeals; i++)
			{
				RoomSeal query = _seals[i];
				if (query.Position.y < y)
				{
					y = query.Position.y;
					seal = query;
				}
			}
			return seal;
		}

		public Vector3 MatchToOthersSeal(RoomSeal other)
		{
			// Remember we are not in the tree yet and cannot
			// change our global position yet
			RoomSeal entrance = FindHighestSeal();
			Vector3 joinPoint = other.GlobalPosition;
			Vector3 newPos = joinPoint - entrance.Position;
			GlobalPosition = newPos;
			GD.Print($"Arena spawning at {newPos} with entrance at {entrance.Position}. Join point {joinPoint} on seal {other.Name}");
			return newPos;
		}

		private void _NextRoom()
		{
			PackedScene arenaType;
			/*if (GD.Randf() > 0.5f)
			{
				arenaType = GD.Load<PackedScene>("res://actors/rooms/room_01.tscn");
			}
			else
			{
				arenaType = GD.Load<PackedScene>("res://actors/rooms/room_02.tscn");
			}*/
			arenaType = GD.Load<PackedScene>("res://actors/rooms/room_01.tscn");

			//PackedScene arenaType = GD.Load<PackedScene>("res://actors/rooms/room_01.tscn");
			Arena next = arenaType.Instantiate<Arena>();
			next.ScanForComponents();
			RoomSeal exit = FindLowestSeal();
			GetParent().AddChild(next);
			Vector3 nextPos = next.MatchToOthersSeal(exit);
			//next.GlobalPosition = nextPos;
		}

		public void TriggerTouched(string name, string message)
		{
			GD.Print($"Arena {Name} - touch message {message} from {name}");
			if (message == "start")
			{
				if (!_running)
				{
					_Start();
				}
				else
				{
					GD.Print($"Arena {Name} is already running!");
				}
			}
		}

		private void _Start()
		{
			_running = true;
			_doors.ForEach(d => d.SetOpen(false));
			for (int i = 0; i < _spawners.Count; i++)
			{
				_spawners[i].Start();
			}
			for (int i = 0; i < _barriers.Count; i++)
			{
				_barriers[i].SetOn(true);
			}
		}

		private void _Finish()
		{
			GD.Print($"Arena {Name} finished");
			_running = false;
			SetProcess(false);
			for (int i = 0; i < _barriers.Count; i++)
			{
				_barriers[i].SetOn(false);
			}
			_doors.ForEach(d => d.SetOpen(true));
			_NextRoom();
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_running)
			{
				int numSpawners = _spawners.Count;
				int numFinishedSpawners = 0;
				for (int i = 0; i < numSpawners;i++)
				{
					numFinishedSpawners += _spawners[i].IsFinished() ? 1 : 0;
				}
				if (numSpawners == numFinishedSpawners)
				{
					_Finish();
				}
			}
		}
	}
}
