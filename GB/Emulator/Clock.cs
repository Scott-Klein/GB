using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator
{
    public class Clock
    {
        private long totalCycles;
        public long Cycles
        {
            get
            {
                return totalCycles;
            }
        }
        private const int TIMER_INTERRUPT_FLAG = 0x2;
        private const int TIMA_ZERO_CYCLES = 4;
        private int _IF;
        public int IF 
        { 
            get
            {
                //Reset the flags when read.
                var flags = _IF;
                _IF = 0;
                return flags;
            }
        }
        private int requestTimaOverflow = 0;
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

        private byte tima;
        public byte TIMA
        {
            get
            {
                return tima;
            }
            set
            {
                //if the TIMA register overflows, it must be
                //reloaded with the value of TMA register.
                //Also a timer interupt is requested.
                if (tima + value > 0xff)
                {
                    requestTimaOverflow = TIMA_ZERO_CYCLES;
                    tima = 0;
                }
                else
                {
                    tima = value;
                }
            }
        }
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
            totalCycles = 0;
        }

        bool andResult = false; //name given by online documentation about gameboy timers.

        public void Tick(int cycles = 1)
        {
            for (int i = 0; i < cycles; i++)
            {
                totalCycles++;
                //div increments every T-cycle
                div++;
                IncrementTIMA();
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


                //TIMA overflow must happen at the END only.
                //handle TIMA overflow.
                if (requestTimaOverflow > 0)
                {
                    if (TIMA != 0 && requestTimaOverflow > 1)
                    {
                        // The over interupt and behaviour can be cancelled.
                        // Writing to TIMA disables.
                        requestTimaOverflow = 0;
                        continue;
                    }
                    requestTimaOverflow--;
                    //if the 4 cycles wait has passed
                    if (requestTimaOverflow == 0)
                    {
                        //let the interupt handler know
                        _IF |= TIMER_INTERRUPT_FLAG;
                        TIMA = TMA;
                    }
                }
            }
        }

        internal byte ReadByte(ushort addr)
        {
            return addr switch
            {
                0xff04 => this.DIV,
                0xff05 => this.TIMA,
                0xff06 => this.TMA,
                0xff07 => this.TAC
            };
        }

        internal void WriteByte(ushort addr, byte value)
        {
            switch (addr)
            {
                case 0xff04:
                    this.DIV = value;
                    break;
                case 0xff05:
                    this.TIMA = value;
                    break;
                case 0xff06:
                    this.TMA = value;
                    break;
                case 0xff07:
                    this.TAC = value;
                    break;
            }
        }

        private void IncrementTIMA()
        {
            //make sure we are not the in overflow behaviour handling.
            // If handling the overflow, it must not be touched for 4 seconds.
            if (requestTimaOverflow == 0)
            {
                tima += (byte)tRate;
            }
        }
    }
}
