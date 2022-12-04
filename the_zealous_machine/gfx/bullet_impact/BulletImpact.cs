using Godot;

namespace TheZealousMachine
{
	public partial class BulletImpact : Node3D
	{
		[Export]
		public float timeToLive = 1f;

		private MeshInstance3D _mesh;
		private GPUParticles3D _particles;
		private OmniLight3D _light;
		private float _tick = 0;
		private Vector3 _originalScale;
		private bool _dead = false;

		public override void _Ready()
		{
			_light = GetNode<OmniLight3D>("OmniLight3D");
			_mesh = GetNode<MeshInstance3D>("MeshInstance3D");
			_particles = GetNode<GPUParticles3D>("GPUParticles3D");
			_particles.Emitting = true;
			_tick = 0;
			_originalScale = Scale;
		}

		public override void _Process(double delta)
		{
			if (_dead) { return; }
			_tick += (float)delta;
			if (_tick > timeToLive)
			{
				_tick = 0;
				_dead = true;
                SetProcess(false);
				QueueFree();
				return;
			}
			float weight = _tick / timeToLive;
			Vector3 newScale = _originalScale.Lerp(Vector3.Zero, weight);
			Scale = newScale;
			_light.OmniRange = Mathf.Lerp(5f, 0f, weight);

		}
	}
}
