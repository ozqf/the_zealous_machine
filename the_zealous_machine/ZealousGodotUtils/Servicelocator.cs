using Godot;

namespace ZealousGodotUtils
{
	public class Servicelocator
	{
		private static Servicelocator? _instance = null;

		public static Servicelocator Get()
		{
			if (_instance == null)
			{
				throw new NullReferenceException("Service Locator is null");
			}
			return _instance;
		}

		public static T Locate<T>()
		{
			if (_instance == null)
			{
				throw new NullReferenceException("Service Locator is null");
			}
			return _instance.GetService<T>();
		}

		public static void SetInstance(Servicelocator instance)
		{
			if (_instance == null)
			{
				GD.Print("Service locator set");
				_instance = instance;
			}
		}

		Dictionary<Type, object> _services = new Dictionary<Type, object>();

		public T GetService<T>()
		{
			Type t = typeof(T);
			if (!_services.ContainsKey(t))
			{
				string msg = $"No service of type {t} found";
				GD.PrintErr(msg);
				throw new NullReferenceException(msg);
			}
			return (T)_services[t];
		}

		public void RegisterService(object obj, Type overrideType = null)
		{
			if (overrideType== null)
			{
				overrideType = obj.GetType();
            }
			GD.Print($"Registered service {overrideType}");
			_services[overrideType] = obj;
		}
	}
}
