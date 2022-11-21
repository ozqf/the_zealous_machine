using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheZealousMachine.gfx
{
    public partial class ShrinkLight : OmniLight3D
    {
        [Export]
        public float timeToLive = 2f;

        private float _tick = 0f;

        private float _originalRadius;

        public override void _Ready()
        {
            _originalRadius = OmniRange;
        }

        public override void _Process(double delta)
        {
            _tick += (float)delta;
            if (_tick >= timeToLive)
            {
                _tick = 0f;
                SetProcess(false);
                Visible = false;
            }
            float weight = 1f - (_tick / timeToLive);
            OmniRange = _originalRadius * weight;
        }
    }
}
