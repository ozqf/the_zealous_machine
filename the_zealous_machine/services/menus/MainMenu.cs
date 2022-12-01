using Godot;
using ZealousGodotUtils;

namespace TheZealousMachine
{
	public partial class MainMenu : Control
	{
		private IGame _game;
		private bool _active = true;

		public void Init(IGame game)
		{
			_game = game;
			SetActive(false);
			GetNode<Button>("VBoxContainer/start").Pressed += _StartClicked;
		}

		private void _StartClicked()
		{
			GlobalEvents.Send(GameEvents.APP_START, null);
		}

		public void SetActive(bool flag)
		{
			_active = flag;
			if (_active )
			{
				Visible = true;
				_game.AddMouseLock("main_menu");
			}
			else
			{
				Visible = false;
				_game.RemoveMouseLock("main_menu");
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			if (Input.IsActionJustPressed("main_menu"))
			{
				SetActive(!_active);
			}
		}
	}
}
