using Godot;

namespace TheZealousMachine.actors.players
{
    public partial class LockOnTarget : Node3D
    {
        private Node3D _realParent;
        private Node3D _target;

        public override void _Ready()
        {
            _realParent = GetParent<Node3D>();
        }

        public bool HasTarget() { return _target != null; }

        public void SetTrackTarget(Node3D node, Vector3 worldPosition)
        {
            if (node == null)
            {
                if (_target != null)
                {
                    _ReturnToRealParent();
                    return;
                }
            }
            _target = node;
            GetParent().RemoveChild(this);
            _target.AddChild(this);
            _target.TreeExiting += _ReturnToRealParent;
            GlobalPosition = worldPosition;
            Visible = true;
        }

        private void _ReturnToRealParent()
        {
            _target = null;
            //Vector3 p = GlobalPosition;
            GetParent().RemoveChild(this);
            _realParent.AddChild(this);
            Visible = false;
            //GlobalPosition = p;
        }
    }
}
