﻿using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine
{
    public partial class HUD : Control
    {
        private PackedScene _itemScene = GD.Load<PackedScene>("res://actors/hud/crosshair_radar_item.tscn");
        private Control _aimPoint;
        private Label _health;

        public override void _Ready()
        {
            _health = GetNode<Label>("health");
            _aimPoint = GetNode<Control>("aim_point");
            GlobalEvents.Register(_OnGlobalEvent);
        }

        public override void _ExitTree()
        {
            GlobalEvents.Unregister(_OnGlobalEvent);
        }

        private void _OnGlobalEvent(string message, object data)
        {
            if (message == GameEvents.HUD_STATUS)
            {
                HudStatus status = data as HudStatus;
                if (status != null)
                {
                    _health.Text = status.health.ToString();
                }
            }
            if (message == GameEvents.MOB_SPAWNED)
            {
                IMob mob = data as IMob;
                if (mob == null)
                {
                    GD.Print("HUD: Mob spawn message data is not a mob!");
                    return;
                }
                HUDRadarItem item = _itemScene.Instantiate<HUDRadarItem>();
                AddChild(item);
                item.SetMob(mob);
                item.SetAimPoint(_aimPoint);
            }
        }
    }
}
