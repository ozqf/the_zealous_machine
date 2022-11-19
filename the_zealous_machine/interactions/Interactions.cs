using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheZealousMachine
{
    public static class Interactions
    {

    }

    public interface IPlayer
    {
        TargetInfo GetTargetInfo();
    }

    public struct TargetInfo
    {
        public Vector3 position;
    }

    public struct ProjectileLaunchInfo
    {
        public Vector3 position;
        public Vector3 forward;
        public float speed;
        public int teamId;
        public int damage;
    }
}
