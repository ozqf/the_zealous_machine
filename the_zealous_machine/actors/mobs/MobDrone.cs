using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.mobs
{
    public partial class MobDrone : CharacterBody3D
    {
        private IGame _game;

        public override void _Ready()
        {
            _game = Servicelocator.Locate<IGame>();
        }
    }
}
