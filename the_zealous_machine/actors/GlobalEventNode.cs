using Godot;
using System.Collections.Generic;
using ZealousGodotUtils;

namespace TheZealousMachine.actors
{
    /*
     * TODO: hmm, probably drop this. Was meant to an
     * global event listener that would handle the
     * subscribe/unsubscribe parts but can't send
     * objects via signals to report what it got.
     */
    public partial class GlobalEventNode : Node
    {
        // hmm, signals cannot send reference types (object in this case)..?
        //[Signal]
        //public delegate void MessageEventHandler(string message, object data);

        public override void _Ready()
        {
            // subscribe
            GlobalEvents.Register(Observe);
        }

        private void _OnTreeExit()
        {
            // unsubscribe
            GlobalEvents.Unregister(Observe);
        }

        public void Observe(string message, object data)
        {

        }
    }
}
