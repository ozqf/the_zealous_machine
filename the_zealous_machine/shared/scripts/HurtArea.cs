using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.shared.scripts
{
    public partial class HurtArea : Area3D
    {
        private CollisionShape3D _shape;
        private HitInfo _hit = new HitInfo();
        private IGame _game;

        public override void _Ready()
        {
            _game = Servicelocator.Locate<IGame>();
            _shape = this.FindFirstChildOfType<CollisionShape3D>();
            this.AreaEntered += _OnAreaEntered;
            this.BodyEntered += _OnBodyEntered;
            _hit.damage = 100;
            _hit.hyper = 1;
            _hit.hitType = HitType.StunImpact;
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
            GD.Print($"Hurt area touched area");
            hittable.Hit(_hit);
        }

        private void _OnBodyEntered(Node node)
        {
            IHittable hittable = node as IHittable;
            if (hittable == null) { return; }
            GD.Print($"Hurt area touched body");
            hittable.Hit(_hit);
            Node3D target = node as Node3D;
            if (target == null) { return; }
            
            Vector3 toward = target.GlobalPosition - GlobalPosition;
            toward = toward.Normalized();
            Vector3 pos = GlobalPosition;// + toward;
            _game.CreateImpactExplosion(pos);
            /*for (int i = 0; i < 8; ++i)
            {
                Vector3 offset = new Vector3();
                offset.x += (float)GD.RandRange(-1, 1);
                offset.y += (float)GD.RandRange(-1, 1);
                offset.z += (float)GD.RandRange(-1, 1);
                _game.CreateBulletImpact(pos + offset, -toward, ImpactType.Purple);
            }*/
        }
    }
}
