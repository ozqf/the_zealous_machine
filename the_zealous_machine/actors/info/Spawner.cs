using Godot;
using System;
using System.Collections.Generic;
using TheZealousMachine.actors.volumes;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.info
{
	public partial class Spawner : Node3D
	{
		private enum SpawnerState { Idle, Spawning, Waiting };

		//[Export]
		//public int maxMobs = 1;
		[Export]
		public int perIterationCount = 1;
		[Export]
		public int iterations = 1;
		[Export]
		public float timeBetweenIterations = 4;
		[Export]
		public float timeBetweenSpawns = 1f;
		[Export]
		public int mobType = -1;
		[Export]
		public Vector3 randPositionOffset = Vector3.Zero;

		private SpawnerState _state = SpawnerState.Idle;
		private Dictionary<Guid, bool> _mobs = new Dictionary<Guid, bool>();

		private IGame _game;
		private bool _active = false;
		private bool _finished = false;

		private float _tick = 0;
		private int _iterations = 0;
		private int _spawnsThisIteration = 0;
		
		public override void _Ready()
		{
			Visible = false;
			GlobalEvents.Register(_OnGlobalEvent);
			_game = Servicelocator.Locate<IGame>();
			SetPhysicsProcess(false);
		}

		private void _OnGlobalEvent(string msg, object data)
		{
			if (!_active) { return; }
			if (msg == GameEvents.MOB_DIED)
			{
				IMob mob = data as IMob;
				if (mob != null && _mobs.ContainsKey(mob.mobId))
				{
					_mobs.Remove(mob.mobId);
				}
			}
		}

		private void _CheckFinished()
		{
			if (_mobs.Count == 0)
			{
				// done
				GD.Print($"Spawner {Name} finished");
				_state = SpawnerState.Idle;
				_active = false;
				_finished = true;
			}
		}

		public void Start()
		{
			if (_state != SpawnerState.Idle)
			{
				GD.Print($"Cannot start spawner - not in idle state!");
				return;
			}
			GD.Print($"Spawner start. Type {mobType}. {iterations}");
			_iterations = 0;
			_spawnsThisIteration = 0;
			_active = true;
			_state = SpawnerState.Spawning;
			SetPhysicsProcess(true);
		}

		public bool IsFinished()
		{
			return _finished;
		}

		private void _SpawnMob()
		{
			Vector3 offset = new Vector3(
				(float)GD.RandRange(-randPositionOffset.x, randPositionOffset.x),
				(float)GD.RandRange(-randPositionOffset.y, randPositionOffset.y),
				(float)GD.RandRange(-randPositionOffset.z, randPositionOffset.z)
			);
			SpawnVolume vol = _game.CreateSpawnVolume(GlobalPosition + offset);
			Guid id = Guid.NewGuid();
			_mobs.Add(id, true);
			vol.mobId = id;
			vol.mobType = (MobType)mobType;
		}

		private void _Tock(float delta)
		{
			switch (_state)
			{
				case SpawnerState.Spawning:
					_tick = timeBetweenSpawns;
					_SpawnMob();
					_spawnsThisIteration += 1;
					if (_spawnsThisIteration >= perIterationCount)
					{
						_spawnsThisIteration = 0;
						_tick = timeBetweenIterations;
						_state = SpawnerState.Waiting;
					}
					break;
				case SpawnerState.Waiting:
					if (_mobs.Count == 0)
					{
						_iterations += 1;
						if (_iterations >= iterations)
						{
							_CheckFinished();
						}
						else
						{
							_state = SpawnerState.Spawning;
							_tick = 0f;
						}
					}
					else
					{
						_tick = timeBetweenIterations;
					}
					break;
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			_tick -= (float)delta;

			// _spawnsThisIteration

			if (_tick <= 0)
			{
				_tick = 1f;
				_Tock((float)delta);
			}

			/*if (_tick <= 0)
			{
				switch (_state)
				{
					case SpawnerState.Spawning:
						break;
				}
				_tick = 15f;
				iterations--;
				//GD.Print($"Spawning {perSpawnCount} mobs");
				for (int i = 0; i < perSpawnCount; ++i)
				{
					Vector3 offset = new Vector3(
						GD.RandRange(-1, 1),
						GD.RandRange(-1, 1),
						GD.RandRange(-1, 1)
					);
					SpawnVolume vol = _game.CreateSpawnVolume(GlobalPosition + offset);
					Guid id = Guid.NewGuid();
					//GD.Print($"\tSpawned {id}");
					_mobs.Add(id, true);
					vol.mobId = id;
					vol.mobType = (MobType)mobType;
				}
			}
			if (iterations <= 0)
			{
				// we are not running but only 'finished'
				// when all mobs are dead.
				SetPhysicsProcess(false);
			}*/
		}
	}
}
