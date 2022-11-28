using Godot;
using System.Collections.Generic;
using ZealousGodotUtils;

namespace TheZealousMachine
{
	public partial class RaycastPylon : Node3D
	{
		private List<RayCast3D> _rays = new List<RayCast3D>();
		private Node3D _near;
		private Node3D _far;
		private Node3D _target;
		private float _lastFraction = 1f;

		public override void _Ready()
		{
			_near = this;
			//_far = GetNode<Node3D>("__far");

			//_target = GetNode<Node3D>("Camera3D");

			int numChildren = this.GetChildCount();
			Godot.Collections.Array<Node> children = this.GetChildren();
			for (int i = 0; i < numChildren; i++)
			{
				var ray = children[i] as RayCast3D;
				if (ray != null)
				{
					_rays.Add(ray);
					continue;
				}
				var node3D = children[i] as Node3D;
				if (node3D != null)
				{
					if (_far == null)
					{
						_far = node3D;
					}
					else
					{
						_target = node3D;
					}
				}    
			}
			if (_far != null)
			{
				Vector3 localPosition = _far.Transform.origin;
				GD.Print($"Setting Ray targets to {localPosition}");
				int numRays = _rays.Count;
				for (int i = 0; i < numRays; i++)
				{
					_rays[i].TargetPosition = localPosition;
				}
			}
			else
			{
				GD.Print($"Raycast pylon {this.Name} has no far target");
			}
			if (_target == null)
			{
				GD.Print($"Raycast pylon {this.Name} has no move target");
			}
		}

		public void OverrideFarPositionTarget(Node3D node3D)
		{
			if (node3D == null) { return; }
			_far = node3D;
		}

		public float GetLastFraction() { return _lastFraction; }

		private float _FindCameraRayFraction()
		{
			float closest = 1f;
			for (int i = 0; i < _rays.Count; i++)
			{
				RayCast3D ray = _rays[i];
				float fraction = ray.GetRayFraction();
				if (fraction < closest)
				{
					closest = fraction;
				}

			}
			return closest;
		}

		public override void _Process(double delta)
		{
			if (_target == null || _far == null) { return; }
			float fraction = _FindCameraRayFraction();
			Transform3D t = _target.Transform;
			t.origin = _near.Transform.origin.Lerp(_far.Transform.origin, fraction);
			_target.Transform = t;
			_lastFraction = fraction;
		}
	}
}
