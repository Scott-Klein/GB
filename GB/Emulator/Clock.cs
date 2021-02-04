namespace GB.Emulator
{
    public class Clock
    {
        private const int TIMA_ZERO_CYCLES = 4;
        private const int TIMER_INTERRUPT_FLAG = 0x2;
        private int _IF;
        private bool andResult = false;
        private ushort div;
        private int requestTimaOverflow = 0;
        private byte tac;
        private byte tima;
        private bool timaEnable;
        private long totalCycles;

        private int tRate;

        public Clock()
        {
            totalCycles = 0;
            tRate = 1024;
        }

        public long Cycles
        {
            get
            {
                return totalCycles;
            }
        }
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

        public byte TMA { get; set; }
        //name given by online documentation about gameboy timers.

        public void Tick(int cycles = 1)
        {
            for (int i = 0; i < cycles; i++)
            {
                totalCycles++;
                //div increments every T-cycle
                div++;

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
                if (andResult && !(d_bit && timaEnable) && div % tRate == 0)
                {
                    tima++;
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
                    if (tima != 0 && requestTimaOverflow > 1)
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
                        tima = TMA;
                    }
                }
            }
        }

        internal byte ReadByte(ushort addr)
        {
            return addr switch
            {
                0xff04 => this.DIV,
                0xff05 => this.tima,
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
                    this.tima = value;
                    break;

                case 0xff06:
                    this.TMA = value;
                    break;

                case 0xff07:
                    this.TAC = value;
                    break;
            }
        }
    }
}