using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator
{
    public class Clock
    {
        private ushort div;
        private int tRate;
        private bool timaEnable;
        public byte DIV
        {
            get
            {
                return (byte)(div >> 8);
            }
            set
            {
                div = 0;
            }
             }

        public byte TIMA { get; set; }
        public byte TMA { get; set; }
        private byte tac;
        public byte TAC
        {
            get
            {
                return tac;
            }
            set
            {
                tac = value;
                switch (value & 3)
                {
                    case 0:
                        tRate = 1024;
                        break;
                    case 1:
                        tRate = 16;
                        break;
                    case 2:
                        tRate = 64;
                        break;
                    case 3:
                        tRate = 256;
                        break;
                }
                timaEnable = (value & 4) > 0;
            }
        }

        public Clock()
        {

        }

        bool andResult = false; //name given by online documentation about gameboy timers.
        public void Tick(int cycles = 1)
        {
            for (int i = 0; i < cycles; i++)
            {
                //choose which bit to take from the div register, by inspecting the tac register.
                int bit = 0;
                switch (tac & 3)
                {
                    case 0:
                        bit = 9;
                        break;
                    case 1:
                        bit = 3;
                        break;
                    case 2:
                        bit = 5;
                        break;
                    case 3:
                        bit = 7;
                        break;
                }
                bool d_bit = ((1 << bit) & div) > 0;

                //we need to & the timer enable with the bit drawn from the div
                if (andResult && !(d_bit && timaEnable))
                {
                    TIMA++;
                    andResult = false;
                }
                else if (d_bit && timaEnable)
                {
                    andResult = true;
                }
            }


        }
    }
}
