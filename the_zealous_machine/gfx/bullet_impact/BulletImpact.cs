using Godot;
using System;
using ZealousGodotUtils;

namespace TheZealousMachine
{
	public partial class BulletImpact : Node3D, IPoolItem
	{
		[Export]
		public float timeToLive = 1f;

		private Pool _pool;
		private Guid _poolItemId;

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
			_originalScale = Scale;
		}

		public override void _PhysicsProcess(double delta)
		{
			if (_dead) { return; }
			_tick += (float)delta;
			if (_tick > timeToLive)
			{
				_pool.RecycleItem(this);
				return;
			}
			float weight = _tick / timeToLive;
			Vector3 newScale = _originalScale.Lerp(Vector3.Zero, weight);
			Scale = newScale;
			_light.OmniRange = Mathf.Lerp(5f, 0f, weight);

		}

		public void RegisterToPool(Pool pool, Guid id)
		{
			_pool = pool;
			_poolItemId = id;
		}

		public Guid GetPoolItemId()
		{
			return _poolItemId;
		}

		public void RestoreForPool()
		{
			_dead = false;
			//_particles.OneShot = true;
			//_particles.Emitting = true;
			_tick = 0;
			Scale = _originalScale;
			_particles.Emitting = true;
			SetPhysicsProcess(true);
		}

		public void RecycleForPool()
		{
			_tick = 0;
			_dead = true;
			//_particles.Emitting = false;
			//_particles.OneShot = false;
			SetPhysicsProcess(false);
		}
	}
}
