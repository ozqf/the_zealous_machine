using Godot;
using ZealousGodotUtils;
using System;

public partial class Main : Node3D
{
	PackedScene _mapBox = GD.Load<PackedScene>("res://maps/box.tscn");
	PackedScene _mapTerainTest1 = GD.Load<PackedScene>("res://maps/terrain_test_01/terrain_test_01.tscn");
	
	// application entry point
	public override void _Ready()
	{
		GD.Print("Main init");
		Servicelocator services = new Servicelocator();
		services.RegisterService(new MouseLock());
		Servicelocator.SetInstance(services);
		Servicelocator.Get().GetService<MouseLock>().AddLock("player");
		LoadEmbeddedMap(_mapTerainTest1);
	}

	private void LoadEmbeddedMap(PackedScene scene)
	{
		Node3D map = scene.Instantiate<Node3D>();
		this.AddChild(map);
	}

	public override void _Process(double delta)
	{
	}
}
