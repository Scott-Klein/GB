namespace GB.Emulator
{
    public class Clock
    {
        private const int TIMA_ZERO_CYCLES = 4;
        private const int TIMER_INTERRUPT_FLAG = 0x4;
        public bool Overflow { get; set; }
        private int _IF;
        private bool andResult = false;
        private ushort div;
        private int timaOverflow = 0;
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

        public byte TIMA 
        { 
            get
            {
                if (timaOverflow != 0)
                {
                    return 0;
                }
                return tima;
            }
            set
            {
                if (tima == 0xff & value == 0)
                {
                    //trigger overflow.
                    timaOverflow = 5;
                    tima = TMA;
                    Overflow = true;
                }
                else
                {
                    tima = (byte)value;
                }
            }
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

                IncrementTIMA();
            }
        }

        bool ANDresult;

        private void IncrementTIMA()
        {
            if (timaOverflow > 0)
            {
                timaOverflow--;
                if (timaOverflow == 0)
                {
                    Overflow = false;
                    this._IF |= 0x04;
                }
            }

            int n = TACbitSelect();

            bool OldResult = ANDresult;
            var mask = (1 << n);
            var mask_Bit = ((mask & div) > 0);
            ANDresult = timaEnable && ((mask & div) > 0);

            if(OldResult && !ANDresult)
            {
                TIMA++;
            }

        }

        private int TACbitSelect()
        {
            return (tac & 3) switch
            {
                0 => 9,
                1 => 3,
                2 => 5,
                3 => 7
            };
        }

        public byte ReadByte(ushort addr)
        {
            return addr switch
            {
                0xff04 => this.DIV,
                0xff05 => this.TIMA,
                0xff06 => this.TMA,
                0xff07 => this.TAC
            };
        }

        public void WriteByte(ushort addr, byte value)
        {
            switch (addr)
            {
                case 0xff04:
                    this.DIV = value;
                    break;

                case 0xff05:
                    if (timaOverflow > 0)
                    {
                        //cancel interupt and the overflow handler.
                        timaOverflow = 0; 
                    }

                    tima = value;
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