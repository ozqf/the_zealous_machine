using Godot;
using System;

namespace TheZealousMachine
{
    public partial class PlayerStart : Node3D
    {
        PackedScene playerTypeA = GD.Load<PackedScene>("res://actors/players/iteration_a/PlayerIterationA.tscn");
        PackedScene playerTypeSixDegrees = GD.Load<PackedScene>("res://actors/players/six_degrees/player_six_degrees.tscn");

        bool _hasSpawned = false;

        public override void _Process(double delta)
        {
            if (_hasSpawned)
            {
                this.SetProcess(false);
                return;
            }
            if (Input.IsActionJustPressed("slot_1"))
            {
                _hasSpawned = true;
                this.SetProcess(false);
                var plyr = playerTypeA.Instantiate<Node3D>();
                this.AddChild(plyr);
                plyr.GlobalTransform = this.GlobalTransform;
            }
            else if (Input.IsActionJustPressed("slot_2"))
            {
                _hasSpawned = true;
                this.SetProcess(false);
                var plyr = playerTypeSixDegrees.Instantiate<Node3D>();
                this.AddChild(plyr);
                plyr.GlobalTransform = this.GlobalTransform;
            }
        }
    }
}

