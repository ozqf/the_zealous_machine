using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.players
{
    public partial class OptionSwing : Node3D
    {
        private float _swingPitchDegrees = 120f;
        private float _swingYawDegrees = 150f;

        private float _tick = 0;
        private float _swingTime = 0.5f;

        public override void _Process(double delta)
        {
            _tick += (float)delta;
            if (_tick >= _swingTime)
            {
                _tick = 0;
            }
            float weight = _tick / _swingTime;
            Rotation = new Vector3
            {
                x = (_swingPitchDegrees * weight * ZGU.DEG2RAD),
                //x = 0,
                y = -(_swingYawDegrees * weight * ZGU.DEG2RAD),
                z = 0
            };
        }
    }
}
