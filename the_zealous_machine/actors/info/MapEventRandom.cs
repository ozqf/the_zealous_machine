using Godot;
using System.Collections.Generic;
using ZealousGodotUtils;

namespace TheZealousMachine.actors.info
{
    public partial class MapEventRandom : Node3D, IMapEvent
    {
        private MapEventState _mapEventState;
        private List<IMapEvent> _mapEvents;
        private int _index;

        public MapEventState mapEventState { get { return _mapEventState; } }

        public override void _Ready()
        {
            _mapEvents = Interactions.FindMapEventChildren(this);
        }

        public void MapEventStart()
        {
            if (_mapEventState != MapEventState.Idle) { return; }
            _mapEventState = MapEventState.Running;
            _index = ZGU.RandomIndex(_mapEvents.Count, GD.Randf());
            _mapEvents[_index].MapEventStart();
            SetPhysicsProcess(true);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_mapEventState != MapEventState.Running)
            {
                SetPhysicsProcess(false);
                return;
            }
            if (_mapEvents[_index].mapEventState == MapEventState.Complete)
            {
                _mapEventState = MapEventState.Complete;
                SetPhysicsProcess(false);
                return;
            }
        }
    }
}
