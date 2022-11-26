using Godot;

namespace TheZealousMachine.actors.mobs.shark
{
    public partial class MobShark : AMob
    {
        private Vector3 _strafeDir = Vector3.Forward;

        public override void _Ready()
        {
            base._Ready();
            _strafeDir.x = (float)GD.RandRange(-1f, 1f);
            _strafeDir.y = (float)GD.RandRange(-1f, 1f);
            _strafeDir.z = (float)GD.RandRange(-1f, 1f);
            _strafeDir = _strafeDir.Normalized();
        }

        protected override void _HuntingTick(float delta)
        {
            Vector3 movePoint = _think.toward + (_strafeDir * 10f);
            _PushMoveToward(movePoint, 20f, 20f, delta);
            _LookInDirectionOfMovement();

            _shootTick -= (float)delta;
            if (_shootTick <= 0)
            {
                _shootTick = _shootTime;
                _Shoot();
            }
        }
    }
}
