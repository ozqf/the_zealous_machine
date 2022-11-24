using Godot;

namespace TheZealousMachine.actors.mobs.gunship
{
    public partial class Gunship : AMob
    {
        public override void _Ready()
        {
            base._Ready();
            _health = 1000;
            _shootTime = 0.05f;
        }

        override protected void _HuntingTick(float delta)
        {
            _LookAtThinkTarget();
            _shootTick -= (float)delta;
            if (_shootTick <= 0)
            {
                _shootTick = _shootTime;
                _Shoot();
            }
        }
    }
}
