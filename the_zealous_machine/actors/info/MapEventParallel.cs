using Godot;
using System.Collections.Generic;

namespace TheZealousMachine.actors.info
{
    public partial class MapEventParallel : Node3D, IMapEvent
    {
        private MapEventState _mapEventState = MapEventState.Idle;
        public MapEventState mapEventState { get { return _mapEventState; } }

        private List<IMapEvent> _mapEvents;

        public override void _Ready()
        {
            SetPhysicsProcess(false);
            _mapEvents = Interactions.FindMapEventChildren(this);
        }

        public void MapEventStart()
        {
            if (_mapEventState != MapEventState.Idle) { return; }
            int numEvents = _mapEvents.Count;
            if (numEvents == 0)
            {
                _mapEventState = MapEventState.Complete;
                return;
            }
            SetPhysicsProcess(true);
            _mapEventState = MapEventState.Running;
            for (int i = 0; i < numEvents; ++i)
            {
                _mapEvents[i].MapEventStart();
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            switch (_mapEventState)
            {
                case MapEventState.Running:
                    int completed = 0;
                    int numEvents = _mapEvents.Count;
                    for (int i = 0; i < numEvents; ++i)
                    {
                        if (_mapEvents[i].mapEventState == MapEventState.Complete)
                        {
                            completed++;
                        }
                    }
                    if (completed >= numEvents)
                    {
                        _mapEventState = MapEventState.Complete;
                        SetPhysicsProcess(false);
                    }
                    break;
            }
        }
    }
}
