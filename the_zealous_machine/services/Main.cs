using Godot;
using ZealousGodotUtils;
using System;
using TheZealousMachine.actors.projectiles;
using TheZealousMachine;
using TheZealousMachine.actors.volumes;
using TheZealousMachine.actors.info;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

public partial class Main : Node3D, IGame
{
	// maps
    private PackedScene _mapBox = GD.Load<PackedScene>("res://maps/box.tscn");
	private PackedScene _mapTerainTest1 = GD.Load<PackedScene>("res://maps/terrain_test_01/terrain_test_01.tscn");
	private PackedScene _mapEndless = GD.Load<PackedScene>("res://maps/endless/endless.tscn");
	
	// projectiles
	private PackedScene _prjGeneric = GD.Load<PackedScene>("res://actors/projectiles/projectile_generic.tscn");
	private PackedScene _prjMobBasic = GD.Load<PackedScene>("res://actors/projectiles/projectile_mob_basic.tscn");
	private PackedScene _prjColumn = GD.Load<PackedScene>("res://actors/projectiles/projectile_column.tscn");
	
	// gfx
    private PackedScene _bulletImpact = GD.Load<PackedScene>("res://gfx/bullet_impact/bullet_impact.tscn");
    private PackedScene _bulletImpactPurple = GD.Load<PackedScene>("res://gfx/bullet_impact/bullet_impact_purple.tscn");
    private PackedScene _bulletImpactGrey = GD.Load<PackedScene>("res://gfx/bullet_impact/bullet_impact_grey.tscn");
    private PackedScene _mobDebrisType = GD.Load<PackedScene>("res://gfx/mob_debris/mob_debris.tscn");

	// info
    private PackedScene _spawnVolume = GD.Load<PackedScene>("res://actors/volumes/spawn_volume.tscn");
    
	// service
	private PackedScene _mainMenuType = GD.Load<PackedScene>("res://services/menus/main_menu.tscn");

    // game
    private IPlayer _player = null;
	private MouseLock _mouseLock = new MouseLock();
	private MainMenu _menu;
	private int _nextActorId = 1;

	private List<Arena> _arenas = new List<Arena>();

	// application entry point
	public override void _Ready()
	{
		GD.Print("Main init");
		// todo - drop this service cack and just do everything through IGame
		Servicelocator services = new Servicelocator();
		services.RegisterService(_mouseLock);
		services.RegisterService(this, typeof(IGame));
		Servicelocator.SetInstance(services);
		_menu = _mainMenuType.Instantiate<MainMenu>();
		AddChild(_menu);
		_menu.Init(this);
		LoadEmbeddedMap(_mapEndless);
	}

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
		Node3D map = scene.Instantiate<Node3D>();
		AddChild(map);
	}

	public IProjectile CreateProjectile(int type = 0)
	{
		IProjectile prj;

		switch (type)
		{
			case 0:
                prj = _prjGeneric.Instantiate<IProjectile>();
				break;
			case 2:
				prj = _prjColumn.Instantiate<IProjectile>();
				break;
			default:
                prj = _prjMobBasic.Instantiate<IProjectile>();
                break;
        }
        
		AddChild(prj.GetPrjBaseNode());
		return prj;
	}

	public Node3D CreateBulletImpact(Vector3 pos, Vector3 directon, ImpactType type = 0)
	{
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
        AddChild(gfx);
        gfx.GlobalPosition = pos;
		gfx.LookAtSafe(pos + directon, Vector3.Up, Vector3.Left);
		return gfx;
	}

	public SpawnVolume CreateSpawnVolume(Vector3 pos)
	{
		SpawnVolume vol = _spawnVolume.Instantiate<SpawnVolume>();
		AddChild(vol);
		vol.GlobalPosition = pos;
		return vol;
	}

	const bool debuggingRooms = false;

	private List<Arena> _pendingArenas = new List<Arena>();

	private Arena AddInterimArena(Transform3D exitSeal)
	{

        PackedScene arenaType = GD.Load<PackedScene>("res://actors/rooms/room_03.tscn");
        Arena arena = arenaType.Instantiate<Arena>();
		AddChild(arena);
        arena.MatchToOthersSeal(exitSeal);
		return arena;
    }

	public void SpawnNextRoom(Transform3D exitSeal, int roomIndex = -1, int arenaIndex = -1)
	{
        PackedScene arenaType;
		int numRooms = 3;

		// spawn runs until we get one that fits.
		int escape = 999;
		while (escape >0)
		{
			escape--;
            if (roomIndex < 0 || roomIndex >= numRooms)
            {
                roomIndex = ZGU.RandomIndex(numRooms, GD.Randf());
            }
            if (debuggingRooms)
            {
                roomIndex = 2;
            }

            switch (roomIndex)
            {
                case 0:
                    arenaType = GD.Load<PackedScene>("res://actors/rooms/room_01.tscn");
                    break;
                case 1:
                    arenaType = GD.Load<PackedScene>("res://actors/rooms/room_02.tscn");
                    break;
                case 2:
                    arenaType = GD.Load<PackedScene>("res://actors/rooms/room_03.tscn");
                    break;
                default:
                    arenaType = GD.Load<PackedScene>("res://actors/rooms/room_01.tscn");
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
				continue;
			}
            AddChild(next);
            if (!next.MatchToOthersSeal(exitSeal))
            {
				// k it said it had a suitable candidate but is lying
                GD.PushError($"Room index {roomIndex} failed to match seal {exitSeal.Forward()}");
                _arenas.ForEach(x => RemoveChild(x));
                _arenas.Clear();
				_player.Reset();
				// reset the index or it will just try this room again!
				roomIndex = -1;
            }
            _arenas.Add(next);
			break;
        }
    }

	public IMob CreateMob(Vector3 pos, MobType type = 0)
	{
		PackedScene scene;
		switch (type)
		{
			case MobType.Gunship:
                scene = GD.Load<PackedScene>("res://actors/mobs/gunship/mob_gunship.tscn");
                break;
            case MobType.Shark:
                scene = GD.Load<PackedScene>("res://actors/mobs/shark/mob_shark.tscn");
                break;
            default:
                scene = GD.Load<PackedScene>("res://actors/mobs/drone/mob_drone.tscn");
				break;
        }
		Node3D mob = scene.Instantiate<Node3D>();
		AddChild(mob);
		mob.GlobalPosition = pos;
		return mob as IMob;
	}

	public Node3D CreateMobDebris(Vector3 pos, Vector3 direction)
	{
        MobDebris debris = _mobDebrisType.Instantiate<MobDebris>();
        AddChild(debris);
        debris.GlobalPosition = pos;
		debris.LookAtSafe(pos + direction, Vector3.Up, Vector3.Left);
		debris.Launch();
        return debris;
    }

    public void RegisterPlayer(IPlayer player)
	{
		_player = player;
	}

	public void UnregisterPlayer(IPlayer player)
	{
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

    public int AssignActorId()
    {
		int result = _nextActorId;
        _nextActorId += 1;
		return result;
    }
}
