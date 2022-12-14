using Godot;
using ZealousGodotUtils;
using System;
using TheZealousMachine.actors.projectiles;
using TheZealousMachine;
using TheZealousMachine.actors.volumes;
using TheZealousMachine.actors.info;
using System.Collections.Generic;
using TheZealousMachine.actors.items;

namespace TheZealousMachine
{
	internal struct RoomSequence
	{
		public int roomIndex;
		public int arenaIndex;
	}
	
	public partial class Main : Node3D, IGame
	{
		enum AppState { Pregame, Playing, Dead }
		// maps
		private PackedScene _mapBox = GD.Load<PackedScene>("res://maps/box.tscn");
		private PackedScene _mapTerainTest1 = GD.Load<PackedScene>("res://maps/terrain_test_01/terrain_test_01.tscn");
		private PackedScene _mapEndless = GD.Load<PackedScene>("res://maps/endless/endless.tscn");

		// projectiles
		private PackedScene _prjGeneric = GD.Load<PackedScene>("res://actors/projectiles/projectile_generic.tscn");
		private PackedScene _prjMobBasic = GD.Load<PackedScene>("res://actors/projectiles/projectile_mob_basic.tscn");
		private PackedScene _prjColumn = GD.Load<PackedScene>("res://actors/projectiles/projectile_column.tscn");
		private PackedScene _prjMobVolume = GD.Load<PackedScene>("res://actors/projectiles/projectile_mob_volume.tscn");

		// mobs
		private PackedScene _mobDrone = GD.Load<PackedScene>("res://actors/mobs/drone/mob_drone.tscn");
		private PackedScene _mobGunship = GD.Load<PackedScene>("res://actors/mobs/gunship/mob_gunship.tscn");
		private PackedScene _mobShark = GD.Load<PackedScene>("res://actors/mobs/shark/mob_shark.tscn");
		private PackedScene _mobCross = GD.Load<PackedScene>("res://actors/mobs/cross/mob_cross.tscn");
		private PackedScene _mobGnawBot = GD.Load<PackedScene>("res://actors/mobs/gnawer/mob_gnawbot.tscn");
		private PackedScene _mobAssaultBot = GD.Load<PackedScene>("res://actors/mobs/assault_bot/mob_assault_bot.tscn");
		private PackedScene _mobBattleshipA = GD.Load<PackedScene>("res://actors/mobs/battleship_a/battleship_a.tscn");

		// gfx
		private PackedScene _bulletImpact = GD.Load<PackedScene>("res://gfx/bullet_impact/bullet_impact.tscn");
		private PackedScene _bulletImpactPurple = GD.Load<PackedScene>("res://gfx/bullet_impact/bullet_impact_purple.tscn");
		private PackedScene _bulletImpactGrey = GD.Load<PackedScene>("res://gfx/bullet_impact/bullet_impact_grey.tscn");
		private PackedScene _mobDebrisType = GD.Load<PackedScene>("res://gfx/mob_debris/mob_debris.tscn");
		private PackedScene _muzzleFlash = GD.Load<PackedScene>("res://gfx/muzzle_flash/muzzle_flash_independent.tscn");
		private PackedScene _impactExplosion = GD.Load<PackedScene>("res://gfx/impact_explosion.tscn");

		// rooms
		private PackedScene _room01 = GD.Load<PackedScene>("res://actors/rooms/room_01.tscn");
		private PackedScene _room02 = GD.Load<PackedScene>("res://actors/rooms/room_02.tscn");
		private PackedScene _room03 = GD.Load<PackedScene>("res://actors/rooms/room_03.tscn");
		private PackedScene _room04 = GD.Load<PackedScene>("res://actors/rooms/room_04.tscn");
		private PackedScene _room05 = GD.Load<PackedScene>("res://actors/rooms/room_05.tscn");
		private PackedScene _room06 = GD.Load<PackedScene>("res://actors/rooms/room_06.tscn");
		private PackedScene _room07 = GD.Load<PackedScene>("res://actors/rooms/room_07.tscn");

		// items
		private PackedScene _quickDrop = GD.Load<PackedScene>("res://actors/items/quick_drop.tscn");
		private PackedScene _quickDropHealth = GD.Load<PackedScene>("res://actors/items/quick_drop_health.tscn");

		// info
		private PackedScene _spawnVolume = GD.Load<PackedScene>("res://actors/volumes/spawn_volume.tscn");

		// service
		private PackedScene _mainMenuType = GD.Load<PackedScene>("res://services/menus/main_menu.tscn");

		private Pool _impactPool;
		// game
		private AppState _state = AppState.Pregame;
		// add current level to this, so main menu can be below it in node order
		private Node3D _mapParent;
		private Node3D _currentMap = null;
		private IPlayer _player = null;
		private MouseLock _mouseLock = new MouseLock();
		private MainMenu _menu;
		private int _nextActorId = 1;
		private List<IMob> _mobs = new List<IMob>();

		private List<RoomSequence> _roomSequence = new List<RoomSequence>
		{
			new RoomSequence { roomIndex = 0, arenaIndex = - 1 },
			new RoomSequence { roomIndex = 0, arenaIndex = - 1 },
			new RoomSequence { roomIndex = 0, arenaIndex = - 1 }
			//new RoomSequence { roomIndex = 1, arenaIndex = - 1 },
			//new RoomSequence { roomIndex = 0, arenaIndex = - 1 },
			//new RoomSequence { roomIndex = 1, arenaIndex = - 1 },
			//new RoomSequence { roomIndex = 0, arenaIndex = - 1 },
		};
		private int _roomSequenceIndex = 0;

		private List<Arena> _arenas = new List<Arena>();
		public int debugRoom = -1;


		// application entry point
		public override void _Ready()
		{
			GD.Print("Main init");
			GlobalEvents.Register(_OnGlobalEvent);
			_mapParent = GetNode<Node3D>("map");
			// todo - drop this service cack and just do everything through IGame
			Servicelocator services = new Servicelocator();
			services.RegisterService(_mouseLock);
			services.RegisterService(this, typeof(IGame));
			Servicelocator.SetInstance(services);
			_menu = _mainMenuType.Instantiate<MainMenu>();
			AddChild(_menu);
			_menu.Init(this);
			_menu.SetActive(true);
			_RestartGame();
			InitBulletImpactPool();
		}

		private void _OnGlobalEvent(string message, object data)
		{
			if (message == GameEvents.APP_START)
			{
				_RestartGame();
			}
		}

		private void _RestartGame()
		{
			_state = AppState.Pregame;
			_roomSequenceIndex = 0;
			LoadEmbeddedMap(_mapEndless);
		}

		public Node3D GetActorRoot() { return _currentMap; }

		public void AddMouseLock(string lockName)
		{
			_mouseLock.AddLock(lockName);
		}

		public void RemoveMouseLock(string lockName)
		{
			_mouseLock.RemoveLock(lockName);
		}

		public bool IsMouseLocked()
		{
			return _mouseLock.IsLocked();
		}

		private void LoadEmbeddedMap(PackedScene scene)
		{
			if (_currentMap != null)
			{
				_mapParent.RemoveChild(_currentMap);
			}
			_currentMap = scene.Instantiate<Node3D>();
			_mapParent.AddChild(_currentMap);
		}

		public void RegisterMob(IMob mob)
		{
			_mobs.Add(mob);
		}

		public void UnregisterMob(IMob mob)
		{
			_mobs.Remove(mob);
		}

		public bool CheckLoS(Vector3 origin, Vector3 target)
		{
			PhysicsRayQueryParameters3D ray = new PhysicsRayQueryParameters3D();
			ray.CollideWithAreas = false;
			ray.CollideWithBodies = true;
			ray.From = origin;
			ray.To = target;
			ray.CollisionMask = 1;
			PhysicsDirectSpaceState3D space = GetWorld3d().DirectSpaceState;
			Godot.Collections.Dictionary result = space.IntersectRay(ray);
			return result.Count == 0;
		}

		public IProjectile CreateProjectile(ProjectileType type)
		{
			IProjectile prj;

			switch (type)
			{
				case ProjectileType.MobBasic:
					prj = _prjMobBasic.Instantiate<IProjectile>();
					break;
				case ProjectileType.PlayerBasic:
					prj = _prjGeneric.Instantiate<IProjectile>();
					break;
				case ProjectileType.Column:
					prj = _prjColumn.Instantiate<IProjectile>();
					break;
				case ProjectileType.MobVolume:
					prj = _prjMobVolume.Instantiate<IProjectile>();
					break;
				default:
					prj = _prjMobBasic.Instantiate<IProjectile>();
					break;
			}

			GetActorRoot().AddChild(prj.GetPrjBaseNode());
			return prj;
		}

		private void InitBulletImpactPool()
		{
			_impactPool = new Pool(_mapParent);
			for (int i = 0; i < 100; ++i)
			{
				BulletImpact gfx = _bulletImpactPurple.Instantiate<BulletImpact>();
				_impactPool.RegisterItemForPool(gfx);
			}
		}

		public void CreateBulletImpact(Vector3 pos, Vector3 directon, ImpactType type = 0)
		{
			IPoolItem item = _impactPool.GetFreeItem();
			if (item == null) { return; }
			BulletImpact gfx = item as BulletImpact;
			gfx.GlobalPosition = pos;
			gfx.LookAtSafe(pos + directon, Vector3.Up, Vector3.Left);

			/*
			BulletImpact gfx;
			switch (type)
			{
				case ImpactType.Purple:
					gfx = _bulletImpactPurple.Instantiate<BulletImpact>();
					break;
				case ImpactType.Grey:
					gfx = _bulletImpactGrey.Instantiate<BulletImpact>();
					break;
				default:
					gfx = _bulletImpact.Instantiate<BulletImpact>();
					break;
			}
			GetActorRoot().AddChild(gfx);
			gfx.GlobalPosition = pos;
			gfx.LookAtSafe(pos + directon, Vector3.Up, Vector3.Left);
			*/
			//return gfx;
		}

		public void CreateMuzzleFlash(Vector3 pos, Vector3 direction)
		{
			Node3D flash = _muzzleFlash.Instantiate<Node3D>();
			GetActorRoot().AddChild(flash);
			flash.GlobalPosition = pos;
			flash.LookAtSafe(pos + direction, Vector3.Up, Vector3.Left);
		}

		public void CreateImpactExplosion(Vector3 pos)
		{
			Node3D gfx = _impactExplosion.Instantiate<Node3D>();
			GetActorRoot().AddChild(gfx);
			gfx.GlobalPosition = pos;
		}

		public void SpawnQuickPickups(Vector3 p, int count = 1, string dropType = "energy")
		{
			PackedScene prefab;
			if (dropType == "health")
			{
				prefab = _quickDropHealth;
			}
			else
			{
				prefab = _quickDrop;
			}
			for (int i = 0; i < count; ++i)
			{
				QuickDrop drop = prefab.Instantiate<QuickDrop>();
				GetActorRoot().AddChild(drop);
				drop.Launch(p);
			}
		}

		public SpawnVolume CreateSpawnVolume(Vector3 pos)
		{
			SpawnVolume vol = _spawnVolume.Instantiate<SpawnVolume>();
			GetActorRoot().AddChild(vol);
			vol.GlobalPosition = pos;
			return vol;
		}

		private Arena AddInterimArena(Transform3D exitSeal)
		{

			PackedScene arenaType = GD.Load<PackedScene>("res://actors/rooms/room_03.tscn");
			Arena arena = arenaType.Instantiate<Arena>();
			GetActorRoot().AddChild(arena);
			arena.MatchToOthersSeal(exitSeal);
			return arena;
		}

		public void SpawnNextRoom(Transform3D exitSeal, int roomIndex = -1, int arenaIndex = -1)
		{
			PackedScene arenaType;
			int numRooms = 7;

			// spawn runs until we get one that fits.
			int escape = 999;
			while (escape > 0)
			{
				escape--;
				if (roomIndex < 0 || roomIndex >= numRooms)
				{
					roomIndex = ZGU.RandomIndex(numRooms, GD.Randf());
				}

				if (_roomSequenceIndex >= 0 && _roomSequenceIndex < _roomSequence.Count)
				{
					roomIndex = _roomSequence[_roomSequenceIndex].roomIndex;
					arenaIndex = _roomSequence[_roomSequenceIndex].arenaIndex;
					_roomSequenceIndex++;
				}

				if (debugRoom >= 0)
				{
					roomIndex = debugRoom;
				}

				switch (roomIndex)
				{
					case 0:
						arenaType = _room01;
						break;
					case 1:
						arenaType = _room02;
						break;
					case 2:
						arenaType = _room03;
						break;
					case 3:
						arenaType = _room04;
						break;
					case 4:
						arenaType = _room05;
						break;
					case 5:
						arenaType = _room06;
						break;
					case 6:
						arenaType = _room07;
						break;
					default:
						arenaType = _room01;
						break;
				}
				Arena next = arenaType.Instantiate<Arena>();
				next.ScanForComponents();
				RoomSeal candiate = next.FindEntranceCandidate(exitSeal);
				// oh dear, try again
				if (candiate == null)
				{
					GD.Print($"Failed to add roomIndex type {roomIndex} trying again...");
					next.Free();
					// reset the index or it will just try this room again!
					roomIndex = -1;
					continue;
				}
				GetActorRoot().AddChild(next);
				if (!next.MatchToOthersSeal(exitSeal))
				{
					// k it said it had a suitable candidate but is lying
					GD.PushError($"Room index {roomIndex} failed to match seal {exitSeal.Forward()}");
					_arenas.ForEach(x => RemoveChild(x));
					_arenas.Clear();
					_player.Reset();
				}
				_arenas.Add(next);
				break;
			}
		}

		public IMob CreateMob(Vector3 pos, MobType type = 0)
		{
			PackedScene scene;
			if ((int)type == -1)
			{
				type = (MobType)ZGU.RandomIndex((int)MobType.LastCommon, GD.Randf());
			}
			// TODO: replace this with a dictionary or array
			switch (type)
			{
				case MobType.Gunship:
					scene = _mobGunship;
					break;
				case MobType.Shark:
					scene = _mobShark;
					break;
				case MobType.Cross:
					scene = _mobCross;
					break;
				case MobType.Gnawbot:
					scene = _mobGnawBot;
					break;
				case MobType.AssaultBot:
					scene = _mobAssaultBot;
					break;
				case MobType.BattleshipA:
					scene = _mobBattleshipA;
					break;
				default:
					scene = _mobDrone;
					break;
			}
			Node3D mobNode = scene.Instantiate<Node3D>();
			GetActorRoot().AddChild(mobNode);
			IMob mob = mobNode as IMob;
			mob.Teleport(pos);
			return mob;
		}

		public Node3D CreateMobDebris(Vector3 pos, Vector3 direction)
		{
			MobDebris debris = _mobDebrisType.Instantiate<MobDebris>();
			GetActorRoot().AddChild(debris);
			debris.GlobalPosition = pos;
			debris.LookAtSafe(pos + direction, Vector3.Up, Vector3.Left);
			debris.Launch();
			return debris;
		}

		public void RegisterPlayer(IPlayer player)
		{
			if (_state == AppState.Pregame)
			{
				_state = AppState.Playing;
			}
			_player = player;
		}

		public void UnregisterPlayer(IPlayer player)
		{
			if (_state == AppState.Pregame)
			{
				_state = AppState.Dead;
			}
			_player = null;
		}

		public TargetInfo GetPlayerTarget()
		{
			if (_player != null)
			{
				return _player.GetTargetInfo();
			}
			TargetInfo info = new TargetInfo();
			info.valid = false;
			return info;
		}

		public bool hasPlayer()
		{
			return _player!= null;
		}

		public IItemCollector GetPlayerCollector() { return _player as IItemCollector; }

		public int AssignActorId()
		{
			int result = _nextActorId;
			_nextActorId += 1;
			return result;
		}

		public void RefreshPaused()
		{
			GetTree().Paused = _menu.Active;
		}
	}
}
