using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.shared.scripts
{
    public partial class HurtArea : Area3D
    {
        private CollisionShape3D _shape;
        private HitInfo _hit = new HitInfo();

        public override void _Ready()
        {
            _shape = this.FindFirstChildOfType<CollisionShape3D>();
            this.AreaEntered += _OnAreaEntered;
            this.BodyEntered += _OnBodyEntered;
            _hit.damage = 100;
            _hit.hyper = 1;
        }

        public void SetOn(bool flag)
        {
            if (flag)
            {
                _shape.Disabled = false;
            }
            else
            {
                _shape.Disabled = true;
            }
        }

        private void _OnAreaEntered(Area3D area)
        {
            IHittable hittable = area as IHittable;
            if (hittable == null) { return; }
            hittable.Hit(_hit);
        }

        private void _OnBodyEntered(Node node)
        {
            IHittable hittable = node as IHittable;
            if (hittable == null) { return; }
            hittable.Hit(_hit);
        }
    }
}
