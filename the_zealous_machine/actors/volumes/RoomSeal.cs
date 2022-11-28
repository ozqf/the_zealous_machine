using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.volumes
{
	//public struct RoomSealInfo
	//{
	//    public Vector3 globalPosition;
	//    public Vector3 localPosition;
	//    public Vector2 size;
	//}

	public partial class RoomSeal : Node3D
	{
		private RayCast3D _ray;
		private OmniLight3D _light;
		private MeshInstance3D _mesh;
		private bool _isEntrance = false;
		private bool _isExit = false;

		public override void _Ready()
		{
			_ray = GetNode<RayCast3D>("RayCast3D");
			_light = GetNode<OmniLight3D>("OmniLight3D");
			_mesh = GetNode<MeshInstance3D>("MeshInstance3D");
			_mesh.Visible = false;
		}

		private void _RefreshLight()
		{
			if (_isExit)
			{
				_light.Visible = true;
				_light.LightColor = Colors.Green;
			}
			else if (isEntrance)
			{
				_light.Visible = true;
				_light.LightColor = Colors.Blue;
			}
			else
			{
				_light.Visible = false;
			}	
		}

		public bool isEntrance
		{
			get
			{
				return _isEntrance;
			}
			set
			{
				_isEntrance = value;
				_RefreshLight();
			}
		}

		public bool isExit
		{
			get
			{
				return _isExit;
			}
			set
			{
				_isExit = value;
				_RefreshLight();
			}
		}

		public bool IsNextToRoom()
		{
			return _ray.IsColliding();
		}

		public bool MatchSeal(Transform3D other)
		{
			Vector3 self;
			if (this.IsInsideTree())
			{
				self = GlobalTransform.basis.z.Normalized();
			}
			else
			{
				self = Transform.basis.z.Normalized();
			}

			Vector3 match = (-other.basis.z).Normalized();
			float dot = (match).Dot(self);
			float diff = dot - 1;
			GD.Print($"Seal dot {dot} (diff {diff}) matching {match} vs self {self}");
			return (Mathf.Abs(diff) < 0.1);
			//return dot == 1;
		}
	}
}
