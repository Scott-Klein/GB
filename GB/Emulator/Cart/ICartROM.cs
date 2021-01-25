using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator.Cart
{
    interface ICartROM
    {
        public byte ReadByte(ushort addr);
        public void WriteByte(ushort addr, byte value);
    }
}
