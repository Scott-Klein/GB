using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator
{
    class Timer
    {
        public byte TIMA;
        public byte TMA;
        public byte TAC;

        internal byte ReadByte(ushort addr)
        {
            throw new NotImplementedException();
        }
    }
}
