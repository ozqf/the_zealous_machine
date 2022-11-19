﻿using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.players
{
    public partial class AimLaser : RayCast3D
    {
        private Node3D _dot;
        public override void _Ready()
        {
            _dot = GetChild<Node3D>(0);
        }

        public override void _Process(double delta)
        {
            //float dt = (float)delta;
            Vector3 dotPos = this.TargetPosition;
            float fraction = this.GetRayFraction();
            if (fraction < 1)
            {
                dotPos = this.TargetPosition * fraction;
            }
            _dot.Position = dotPos;
        }
    }
}