using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.volumes
{
	public partial class TriggerTouch : Area3D
	{
		[Export]
		private string _message = "";
        ITriggerable _triggerTarget;
		[Export]
		private bool _active = true;

		public override void _Ready()
		{
			GetNode<MeshInstance3D>("MeshInstance3D").Visible = false;
			_triggerTarget = this.FindParentOfTypeRecursive<ITriggerable>() as ITriggerable;
			if (_triggerTarget == null )
			{
				GD.Print($"Touch Triger {Name} found no target parent");
				_active = false;
				return;
			}
			this.BodyEntered += _OnBodyEntered;
		}

		private void _OnBodyEntered(Node body)
		{
			if (!_active) { return; }
			if (_triggerTarget != null)
			{
				_active = false;
				_triggerTarget.Trigger(Name, _message);
			}
		}
	}
}
