using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator
{
    public class Joypad
    {

        public Joypad()
        {

        }
        private const int JOY_INTERRUPT = 0x10;
        private const int RIGHT_KEY = 0x1;
        private const int LEFT_KEY = 0x2;
        private const int UP_KEY = 0x4;
        private const int DOWN_KEY = 0x8;
        private const int A_KEY = 0x1;
        private const int B_KEY = 0x2;
        private const int SELECT_KEY = 0x4;
        private const int START_KEY = 0x8;
        private const int BUTTON_MODE = 0x10;
        private const int DIRECTION_MODE = 0x20;

        public byte Joy1 
        { 
            get
            {
                int result = 0xc0;
                if (DirectionKeys)
                {
                    result |= DIRECTION_MODE;
                    result = Right ? result  : RIGHT_KEY | result;
                    result = Left ? result  : LEFT_KEY | result;
                    result = Up ? result : UP_KEY | result;
                    result = Down ? result : DOWN_KEY | result;
                }
                else
                {
                    result |= BUTTON_MODE;
                    result = A ? result : A_KEY | result;
                    result = B ? result : B_KEY | result;
                    result = Select ? result : SELECT_KEY | result;
                    result = Start ? result  : START_KEY | result;
                }
                if ((result & 0xf) < 0xf)
                {
                    //raise interrupt.
                    EventHandler interruptHandler = JoyPadInterrupt;
                    if (interruptHandler is not null)
                    {
                        JoyPadInterrupt(this, new EventArgs());
                    }
                }
                return (byte)result;
            }
            set
            {
                if ((value & 0x10) > 0 )
                {
                    DirectionKeys = false;
                }
                else if ((value & 0x20) > 0)
                {
                    DirectionKeys = true;
                }
            }
        }

        public void Reset()
        {
            Start   = false;
            Select  = false;
            A       = false;
            B       = false;
            Up      = false;
            Down    = false;
            Left    = false;
            Right   = false;

        }

        public event EventHandler JoyPadInterrupt;

        private bool DirectionKeys;

        public bool Start;
        public bool Select;
        public bool A;
        public bool B;
        public bool Up;
        public bool Down;
        public bool Left;
        public bool Right;
    }
}
