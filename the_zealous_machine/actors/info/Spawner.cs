using Godot;
using System.Collections.Generic;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.info
{
    public partial class Spawner : Node3D
    {
        [Export]
        public int maxMobs = 10;
        [Export]
        public int perSpawnCount = 1;

        private List<Node3D> _nodes = new List<Node3D>();

        private IGame _game;
        private bool _active = false;

        private float _tick = 0;

        public override void _Ready()
        {
            _game = Servicelocator.Locate<IGame>();
            SetProcess(false);
        }

        public void Start()
        {
            GD.Print("Spawner start");
            _active = true;
            SetProcess(true);
        }

        public override void _Process(double delta)
        {
            _tick -= (float)delta;
            if (_tick <= 0)
            {
                _tick = 10f;
                GD.Print($"Spawning {perSpawnCount} mobs");
                for (int i = 0; i < perSpawnCount; ++i)
                {
                    Vector3 offset = new Vector3(
                        GD.RandRange(-1, 1),
                        GD.RandRange(-1, 1),
                        GD.RandRange(-1, 1)
                    );
                    _game.CreateSpawnVolume(GlobalPosition + offset);
                }
            }
        }
    }
}
