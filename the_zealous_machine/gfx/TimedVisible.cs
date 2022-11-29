using Godot;

namespace TheZealousMachine
{
	public partial class TimedVisible : Node3D
	{
		private float _tick = 0f;
		private float _flashTick = 0f;
		public bool flash = false;
		public float flashTime = 0.05f;
		
		public void Run(float time)
		{
			_tick = time;
			_flashTick = flashTime;
			Visible = true;
			SetProcess(true);
		}

		public override void _Process(double delta)
		{
			_tick -= (float)delta;
			if (flash)
			{
				_flashTick -= (float)delta;
				if (_flashTick <= 0f)
				{
					Visible = !Visible;
					_flashTick = flashTime;
				}
			}
			if (_tick <= 0f)
			{
				SetProcess(false);
				Visible= false;
			}
		}
	}
}
