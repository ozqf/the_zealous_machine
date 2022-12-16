using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.players
{
    public partial class UIBar3D : Node3D
    {
        public enum StatusField { Health, Ammo };
        [Export]
        public StatusField status = StatusField.Health;
        [Export]
        public int maxValue = 100;

        private int _value = 0;
        private Node3D _bar;
        public override void _Ready()
        {
            _bar = GetNode<Node3D>("bar_mesh");
            GlobalEvents.Register(_OnGlobalMessage);
            _Refresh();
        }

        public override void _ExitTree()
        {
            GlobalEvents.Unregister(_OnGlobalMessage);
        }

        private void _Refresh()
        {
            float weight = (float)_value / (float)maxValue;
            _bar.Scale = new Vector3(1, weight, 1);
        }

        private void _OnGlobalMessage(string message, object data)
        {
            if (message != GameEvents.HUD_STATUS) { return; }
            HudStatus hud = data as HudStatus;
            if (hud == null) { return; }
            switch (status)
            {
                case StatusField.Health:
                    _value = hud.health;
                    break;
                case StatusField.Ammo:
                    _value = hud.ammo;
                    break;
            }
            _Refresh();
        }
    }
}
