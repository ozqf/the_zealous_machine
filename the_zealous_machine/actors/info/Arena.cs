using Godot;

namespace TheZealousMachine.actors.info
{
    public partial class Arena : Node3D, IArena
    {
        public void TriggerTouched(string name, string message)
        {
            GD.Print($"Arena {Name} - touch message {message} from {name}");
        }
    }
}
