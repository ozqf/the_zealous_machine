using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine
{
	public partial class HUDRadarItem : Control
	{
		private Control _aimPoint;
		private IMob _mob = null;

		public override void _Ready()
		{
			GlobalEvents.Register(_OnGlobalEvent);
			//Rotation = GD.RandRange(0, 360);
		}

		public void SetMob(IMob mob)
		{
			this._mob = mob;
		}

		public void SetAimPoint(Control aimPoint)
		{
			this._aimPoint = aimPoint;
		}

		private void _OnGlobalEvent(string message, object data)
		{
			if (message == GameEvents.MOB_DIED && data == _mob)
			{
				QueueFree();
			}
		}

		public override void _Process(double delta)
		{
			Vector3 subject = _mob.GetBaseNode().GlobalPosition;
			Camera3D camera = GetViewport().GetCamera3d();
			Transform3D cameraT = camera.GlobalTransform;
			Vector3 toward = (subject - cameraT.origin).Normalized();
			float dot = toward.Dot(cameraT.Forward());
			if (dot > 0)
			{
				Visible = false;
				return;
			}
			Visible = true;
			Vector2 screenPos = camera.UnprojectPosition(subject);
			float radians = _aimPoint.GlobalPosition.AngleTo(screenPos);
			Rotation = radians;
		}
	}
}
