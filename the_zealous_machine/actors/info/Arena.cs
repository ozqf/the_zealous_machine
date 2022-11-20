using Godot;
using System.Collections.Generic;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.info
{
    public partial class Arena : Node3D, IArena
    {
        private List<Spawner> _spawners;
        public override void _Ready()
        {
            _spawners = this.FindChildrenOfType<Spawner>();
            GD.Print($"Arena found {_spawners.Count} spawners");
        }

        public void TriggerTouched(string name, string message)
        {
            GD.Print($"Arena {Name} - touch message {message} from {name}");
            for (int i = 0; i < _spawners.Count; i++)
            {
                _spawners[i].Start();
            }
        }
    }
}
