using Godot;

namespace TheZealousMachine.shared.scripts
{
    public partial class ImmediateAnimationPlay : AnimationPlayer
    {
        [Export]
        public string animationName = "";
        public override void _Ready()
        {
            Play(animationName);
        }
    }
}
