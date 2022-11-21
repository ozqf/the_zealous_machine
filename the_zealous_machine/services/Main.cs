using Godot;
using ZealousGodotUtils;
using System;
using TheZealousMachine.actors.projectiles;
using TheZealousMachine;
using TheZealousMachine.actors.volumes;

public partial class Main : Node3D, IGame
{
    private PackedScene _mapBox = GD.Load<PackedScene>("res://maps/box.tscn");
	private PackedScene _mapTerainTest1 = GD.Load<PackedScene>("res://maps/terrain_test_01/terrain_test_01.tscn");
	private PackedScene _prjGeneric = GD.Load<PackedScene>("res://actors/projectiles/projectile_generic.tscn");
	private PackedScene _bulletImpact = GD.Load<PackedScene>("res://gfx/bullet_impact/bullet_impact.tscn");
	private PackedScene _mobDebrisType = GD.Load<PackedScene>("res://gfx/mob_debris/mob_debris.tscn");
	private PackedScene _mainMenuType = GD.Load<PackedScene>("res://services/menus/main_menu.tscn");

	private PackedScene _spawnVolume = GD.Load<PackedScene>("res://actors/volumes/spawn_volume.tscn");

	private IPlayer _player = null;
	private MouseLock _mouseLock = new MouseLock();
	private MainMenu _menu;
	private int _nextActorId = 1;

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
		LoadEmbeddedMap(_mapTerainTest1);
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

	public ProjectileGeneric CreateProjectile()
	{
		ProjectileGeneric prj = _prjGeneric.Instantiate<ProjectileGeneric>();
		AddChild(prj);
		return prj;
	}

	public Node3D CreateBulletImpact(Vector3 pos, Vector3 directon)
	{
		BulletImpact gfx = _bulletImpact.Instantiate<BulletImpact>();
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

	public Node3D CreateMobDrone(Vector3 pos)
	{
		PackedScene scene = GD.Load<PackedScene>("res://actors/mobs/drone/mob_drone.tscn");
		Node3D mob = scene.Instantiate<Node3D>();
		AddChild(mob);
		mob.GlobalPosition = pos;
		return mob;
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
