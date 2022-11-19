using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.volumes
{
	public partial class TriggerTouch : Area3D
	{
		[Export]
		private string _message = "";
		IArena _arena;
		private bool _active = true;

		public override void _Ready()
		{
			GetNode<MeshInstance3D>("MeshInstance3D").Visible = false;
			_arena = this.FindParentOfTypeRecursive<IArena>() as IArena;
			if (_arena == null )
			{
				GD.Print($"Touch Triger {Name} found no arena parent");
				_active = false;
				return;
			}
			this.BodyEntered += _OnBodyEntered;
		}

		private void _OnBodyEntered(Node body)
		{
			if (!_active) { return; }
			_arena.TriggerTouched(Name, _message);
		}
	}
}
