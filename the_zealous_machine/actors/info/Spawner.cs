using Godot;
using System;
using System.Collections.Generic;
using TheZealousMachine.actors.volumes;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.info
{
    public partial class Spawner : Node3D
    {
        [Export]
        public int maxMobs = 10;
        [Export]
        public int perSpawnCount = 1;
        [Export]
        public int iterations = 1;

        //private List<Node3D> _nodes = new List<Node3D>();
        private Dictionary<Guid, bool> _mobs = new Dictionary<Guid, bool>();

        private IGame _game;
        private bool _active = false;
        private bool _finished = false;

        private float _tick = 0;
        
        public override void _Ready()
        {
            GlobalEvents.Register(_OnGlobalEvent);
            _game = Servicelocator.Locate<IGame>();
            SetProcess(false);
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
                    _CheckFinished();
                }
            }
        }

        private void _CheckFinished()
        {
            if (_mobs.Count == 0)
            {
                // done
                GD.Print($"Spawner {Name} finished");
                _active = false;
                _finished = true;
            }
        }

        public void Start()
        {
            GD.Print("Spawner start");
            _active = true;
            SetProcess(true);
        }

        public bool IsFinished()
        {
            return _finished;
        }

        public override void _Process(double delta)
        {
            _tick -= (float)delta;
            if (_tick <= 0)
            {
                _tick = 15f;
                iterations--;
                GD.Print($"Spawning {perSpawnCount} mobs");
                for (int i = 0; i < perSpawnCount; ++i)
                {
                    Vector3 offset = new Vector3(
                        GD.RandRange(-1, 1),
                        GD.RandRange(-1, 1),
                        GD.RandRange(-1, 1)
                    );
                    SpawnVolume vol = _game.CreateSpawnVolume(GlobalPosition + offset);
                    Guid id = Guid.NewGuid();
                    GD.Print($"\tSpawned {id}");
                    _mobs.Add(id, true);
                    vol.mobId = id;

                }
            }
            if (iterations <= 0)
            {
                // we are not running but only 'finished'
                // when all mobs are dead.
                SetProcess(false);
            }
        }
    }
}
