using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine
{
	public partial class HUDRadarItem : Control
	{
		private Control _iconSpoke;
		private Control _icon;
		private Control _retacle;
		private Label _label;
		private Control _aimPoint;
		private IMob _mob = null;

		public override void _Ready()
		{
			_iconSpoke = GetNode<Control>("icon");
			_icon = GetNode<Control>("icon/ColorRect");
			_retacle = GetNode<Control>("screen_position");
			_label = GetNode<Label>("screen_position/Label");
			_label.Visible = false;
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
			Vector2 screenPos = camera.UnprojectPosition(subject);
			Transform3D cameraT = camera.GlobalTransform;
			Vector3 toward = (subject - cameraT.origin).Normalized();
			float distSqr = subject.DistanceSquaredTo(cameraT.origin);
			float dot = toward.Dot(cameraT.Forward());
			if (dot > 0.5f)
			{
				_retacle.Visible = true;
				_retacle.GlobalPosition = screenPos;
				_iconSpoke.Visible = false;
				//Visible = false;
				return;
			}
			_retacle.Visible = false;
			_iconSpoke.Visible = true;
			//Visible = true;
			Control posNode = GetNode<Control>("screen_position");
			posNode.GlobalPosition = screenPos;
			Vector2 between = screenPos - _aimPoint.GlobalPosition;
			float radians = between.Angle();
			//float radians = _aimPoint.GlobalPosition.AngleTo(screenPos);
			//float radians = screenPos.AngleTo(_aimPoint.GlobalPosition);
			if (dot > 0)
			{
				radians += 90f * ZGU.DEG2RAD;
			}
			else
			{
				radians += 270f * ZGU.DEG2RAD;
			}

			float weight = distSqr / (25f * 25f);
			weight = Mathf.Clamp(weight, 0.0f, 1.0f);
			Vector2 max = new Vector2(3f, 3f);
			Vector2 min = new Vector2(0.5f, 0.5f);
			_icon.Scale = max.Lerp(min, weight);
			_iconSpoke.Rotation = radians;
			//_label.Text = $"{radians * ZGU.RAD2DEG}, aim pos {_aimPoint.GlobalPosition} vs {screenPos}";
		}
	}
}
