using Godot;
using System.Collections.Generic;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.info
{
    public partial class Arena : Node3D, IArena
    {
        private List<Spawner> _spawners;
        private List<PlayerBarrier> _barriers;
        private bool _running = false;
        public override void _Ready()
        {
            _spawners = this.FindChildrenOfType<Spawner>();
            _barriers = this.FindChildrenOfType<PlayerBarrier>();
            GD.Print($"Arena {Name} found {_spawners.Count} spawners and {_barriers.Count} barriers");
        }

        public void TriggerTouched(string name, string message)
        {
            if (_running)
            {
                GD.Print($"Arena {Name} is already running!");
                return;
            }
            GD.Print($"Arena {Name} - touch message {message} from {name}");
            _Start();
        }

        private void _Start()
        {
            _running = true;
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
