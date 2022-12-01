using System.Collections.Generic;


namespace ZealousGodotUtils
{
    public delegate void GlobalEvent(string msg, object data);


    public static class GlobalEvents
    {
        private static List<GlobalEvent> _listeners = new List<GlobalEvent>();

        public static void Register(GlobalEvent listener)
        {
            _listeners.Add(listener);
        }

        public static void Unregister(GlobalEvent listener)
        {
            _listeners.Remove(listener);
        }

        public static void Send(string msg, object data)
        {
            // TODO: This is not safe if listeners Unregister during send!
            // also if listeners are added during send it will extend.
            for (int i = 0; i < _listeners.Count; i++)
            {
                _listeners[i].Invoke(msg, data);
            }
        }
    }
}
