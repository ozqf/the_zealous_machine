using Godot;
using System.Collections.Generic;

namespace TheZealousMachine.actors.info
{
    public partial class MapEventSequence : Node3D, IMapEvent
    {
        private MapEventState _mapEventState = MapEventState.Idle;
        public MapEventState mapEventState { get { return _mapEventState; } }

        private List<IMapEvent> _mapEvents;
        private int _mapEventIndex = -1;

        public override void _Ready()
        {
            SetPhysicsProcess(false);
            _mapEvents = Interactions.FindMapEventChildren(this);
        }

        public void MapEventStart()
        {
            if (_mapEventState != MapEventState.Idle) { return; }
            if (_mapEvents.Count == 0)
            {
                _mapEventState = MapEventState.Complete;
                return;
            }
            _mapEventState = MapEventState.Running;
            _mapEventIndex = 0;
            SetPhysicsProcess(true);
            GD.Print($"Sequence starting index {_mapEventIndex} of {_mapEvents.Count}");
            _mapEvents[_mapEventIndex].MapEventStart();
        }

        public override void _PhysicsProcess(double delta)
        {
            switch (_mapEventState)
            {
                case MapEventState.Running:
                    if (_mapEvents[_mapEventIndex].mapEventState == MapEventState.Complete)
                    {
                        GD.Print($"Event {_mapEventIndex} finished, incrementing...");
                        _mapEventIndex += 1;
                        if (_mapEventIndex >= _mapEvents.Count)
                        {
                            _mapEventState = MapEventState.Complete;
                            SetPhysicsProcess(false);
                        }
                        else
                        {
                            GD.Print($"Sequence starting next index {_mapEventIndex} of {_mapEvents.Count}");
                            _mapEvents[_mapEventIndex].MapEventStart();
                        }
                    }
                    break;
                default:
                    return;
            }
        }
    }
}
