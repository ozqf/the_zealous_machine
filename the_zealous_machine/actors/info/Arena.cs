using Godot;
using System.Collections.Generic;
using TheZealousMachine.actors.volumes;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.info
{
	public partial class Arena : Node3D, IArena, ITriggerable
	{
		private bool _scanned = false;
		private List<IMapEvent> _mapEvents;
		private int _mapEventIndex = -1;
		private List<PlayerBarrier> _barriers;
		//private List<IDoor> _doors;
		private List<RoomSeal> _seals;
		private bool _running = false;

		public override void _Ready()
		{
			ScanForComponents();
		}

		public void Trigger(string name, string message)
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

		public void ScanForComponents()
		{
			if (_scanned) { return; }
			_scanned = true;
			_mapEvents = Interactions.FindMapEventChildren(this);
			_barriers = this.FindChildrenOfType<PlayerBarrier>();
			//_doors = new List<IDoor>();
			//_doors = this.FindChildrenOfType<IDoor>();
			_seals = this.FindChildrenOfType<RoomSeal>();
			GD.Print($"Arena {Name} found {_mapEvents.Count} spawners, {_seals.Count} seals, {_barriers.Count} barriers");
			//_doors.ForEach(d => d.SetOpen(true));
		}

		private bool _IsValidExitNormal(Vector3 query)
		{
			if (query.Dot(Vector3.Up) > 0.9f) { return false; }
			if (query.Dot(Vector3.Left) > 0.9f) { return false; }
			if (query.Dot(Vector3.Back) > 0.9f) { return false; }
			return true;
		}

		public RoomSeal PickExit()
		{
			int escape = 999;
			int numSeals = _seals.Count;
			while (escape > 0)
			{
				escape--;
				int i = ZGU.RandomIndex(numSeals, GD.Randf());
				RoomSeal candidate = _seals[i];
				if (candidate.isEntrance
					|| candidate.IsNextToRoom()
					|| !_IsValidExitNormal(candidate.ForwardGlobal()))
				{
					continue;
				}
				candidate.isExit = true;
				return candidate;
			}
			return null;
		}

		public RoomSeal FindEntranceCandidate(Transform3D other)
		{
			int numSeals = _seals.Count;
			for (int i = 0; i < numSeals; ++i)
			{
				RoomSeal seal = _seals[i];
				if (seal.MatchSeal(other))
				{
					return seal;
				}
			}
			return null;
		}

		public RoomSeal PickEntrance(Transform3D other)
		{
			int numSeals = _seals.Count;
			for (int i = 0; i < numSeals; ++i)
			{
				RoomSeal seal = _seals[i];
				if (seal.MatchSeal(other))
				{
					seal.isEntrance = true;
					return seal;
				}
			}
			return null;
		}

		public bool MatchToOthersSeal(Transform3D other)
		{
			// Remember we are not in the tree yet and cannot
			// change our global position yet
			RoomSeal entrance = PickEntrance(other);
			if (entrance == null)
			{
				return false;
			}
			Vector3 joinPoint = other.origin;
			Vector3 newPos = joinPoint - entrance.Position;
			GlobalPosition = newPos;
			GD.Print($"Arena joining to other forward: {-other.basis.z}");
			//GD.Print($"Arena spawning at {newPos} with entrance at {entrance.Position}. Join point {joinPoint}");
			return true;
		}

		private void _NextRoom()
		{
			RoomSeal exit = PickExit();
			Servicelocator.Locate<IGame>().SpawnNextRoom(exit.GlobalTransform);
		}

		private void _Start()
		{
			_running = true;
			//_doors.ForEach(d => d.SetOpen(false));
			_seals.ForEach(s => s.SetForceClosed(true));
			for (int i = 0; i < _mapEvents.Count; i++)
			{
				_mapEvents[i].MapEventStart();
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
			//_doors.ForEach(d => d.SetOpen(true));
			// unseal everything that isn't the entrance
			_seals.ForEach(s => { if (!s.isEntrance) { s.SetForceClosed(false); } });
			_NextRoom();
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_running)
			{
				int numSpawners = _mapEvents.Count;
				int numFinishedSpawners = 0;
				for (int i = 0; i < numSpawners;i++)
				{
					numFinishedSpawners += _mapEvents[i].mapEventState == MapEventState.Complete ? 1 : 0;
				}
				if (numSpawners == numFinishedSpawners)
				{
					_Finish();
				}
			}
		}
	}
}
