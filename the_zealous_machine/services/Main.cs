using Godot;
using ZealousGodotUtils;
using System;
using TheZealousMachine.actors.projectiles;
using TheZealousMachine;

public partial class Main : Node3D, IGame
{
    private PackedScene _mapBox = GD.Load<PackedScene>("res://maps/box.tscn");
	private PackedScene _mapTerainTest1 = GD.Load<PackedScene>("res://maps/terrain_test_01/terrain_test_01.tscn");
	private PackedScene _prjGeneric = GD.Load<PackedScene>("res://actors/projectiles/projectile_generic.tscn");

	private PackedScene _mainMenuType = GD.Load<PackedScene>("res://services/menus/main_menu.tscn");

	private IPlayer _player = null;
	private MouseLock _mouseLock = new MouseLock();
	private MainMenu _menu;

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
		this.AddChild(map);
	}

	public ProjectileGeneric CreateProjectile()
	{
		ProjectileGeneric prj = _prjGeneric.Instantiate<ProjectileGeneric>();
		AddChild(prj);
		return prj;
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

	public override void _Process(double delta)
	{
	}
}
