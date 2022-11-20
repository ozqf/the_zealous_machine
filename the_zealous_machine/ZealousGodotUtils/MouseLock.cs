using Godot;

namespace ZealousGodotUtils
{
	public class MouseLock
	{
		private Dictionary<string, bool> _locks = new Dictionary<string, bool>();

		private void RefreshLock()
		{
			if (_locks.Any())
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
				GD.Print("Mouse free");
			}
			else
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
				GD.Print("Mouse captured");
			}
		}

		public bool IsLocked() { return _locks.Any(); }

		public void AddLock(string key)
		{
			_locks[key] = true;
			RefreshLock();
		}

		public void RemoveLock(string key)
		{
			_locks.Remove(key);
			RefreshLock();
		}
	}
}
