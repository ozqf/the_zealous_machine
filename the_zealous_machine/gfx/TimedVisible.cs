using Godot;

namespace TheZealousMachine
{
	public partial class TimedVisible : Node3D
	{
		private float _tick = 0f;
		
		public void Run(float time)
		{
			_tick = time;
			Visible = true;
			SetProcess(true);
		}

		public override void _Process(double delta)
		{
			_tick -= (float)delta;
			if (_tick <= 0f)
			{
				SetProcess(false);
				Visible= false;
			}
		}
	}
}
