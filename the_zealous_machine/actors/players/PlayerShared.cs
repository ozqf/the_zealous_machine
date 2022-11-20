using Godot;

namespace TheZealousMachine
{
    public struct PlayerInput
    {
        public static PlayerInput Empty { get { return new PlayerInput(); } }

        public Vector3 pushAxes;
        public Vector3 roll;
        public bool boosting;
        public bool gliding;
        public bool attack1;
        public bool attack2;

        public bool slot1;
        public bool slot2;
        public bool slot3;
        public bool slot4;
    }

    public sealed class PlayerDebugInfo
    {
        public Vector3 lastMoveContactPosition;
        public Vector3 lastMoveContactNormal;
    }

    public static class PlayerUtils
    {
        public const float DEG2RAD = 0.017453292519f;
        public const float RAD2DEG = 57.29577951308f;

        public static Vector3 InputAxesToCharacterPush(Vector3 pushAxes, Basis pushBasis)
        {
            Vector3 push = new Vector3();
            push += (-pushBasis.z) * pushAxes.z;
            push += pushBasis.y * pushAxes.y;
            push += (pushBasis.x) * pushAxes.x;
            return push;
        }

        public static PlayerInput GetInput()
        {
            PlayerInput input = new PlayerInput();
            if (Input.IsActionPressed("move_forward"))
            {
                input.pushAxes.z += 1;
            }
            if (Input.IsActionPressed("move_backward"))
            {
                input.pushAxes.z -= 1;
            }
            if (Input.IsActionPressed("move_left"))
            {
                input.pushAxes.x -= 1;
            }
            if (Input.IsActionPressed("move_right"))
            {
                input.pushAxes.x += 1;
            }
            if (Input.IsActionPressed("move_up"))
            {
                input.pushAxes.y += 1;
            }
            if (Input.IsActionPressed("move_down"))
            {
                input.pushAxes.y -= 1;
            }
            if (Input.IsActionPressed("roll_left"))
            {
                input.roll.z += 1;
            }
            if (Input.IsActionPressed("roll_right"))
            {
                input.roll.z -= 1;
            }
            input.boosting = Input.IsActionPressed("boost");
            input.gliding = Input.IsActionPressed("glide");
            input.attack1 = Input.IsActionPressed("attack_1");
            input.attack2 = Input.IsActionPressed("attack_2");
            input.slot1 = Input.IsActionPressed("slot_1");
            input.slot2 = Input.IsActionPressed("slot_2");
            input.slot3 = Input.IsActionPressed("slot_3");
            input.slot4 = Input.IsActionPressed("slot_4");
            return input;
        }
    }
}
