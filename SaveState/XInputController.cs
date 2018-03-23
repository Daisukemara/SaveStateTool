using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XInput;

namespace SaveState
{
    class XInputController
    {
        Controller controller;
        public bool connection = false;
        public int deadband = 2500;
        public GamepadButtonFlags leftBumper;
        public GamepadButtonFlags rightBumper;

        State oldState;

        public XInputController()
        {
            controller = new Controller(UserIndex.One);
            connection = controller.IsConnected;
            leftBumper = GamepadButtonFlags.LeftShoulder;
            rightBumper = GamepadButtonFlags.RightShoulder;
        }

        public int GetInput()
        {
            State newState = controller.GetState();
            int value = -1;

            if (oldState.Gamepad.Buttons.HasFlag(leftBumper) && newState.Gamepad.Buttons.HasFlag(leftBumper) && newState.Gamepad.Buttons != GamepadButtonFlags.None)
            {
                value = 0;
            }
            else if (oldState.Gamepad.Buttons.HasFlag(rightBumper) && newState.Gamepad.Buttons.HasFlag(rightBumper) && newState.Gamepad.Buttons != GamepadButtonFlags.None)
            {
                value = 1;
            }

            oldState = newState;

            return value;
        }
    }
}
