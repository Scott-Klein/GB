using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator.Cart
{
    class StandardROM : ICartROM
    {
        public StandardROM(byte[] rom)
        {
            ROM = rom;
        }
        public byte[] ROM { get; set; }

        public byte ReadByte(ushort addr)
        {
            return ROM[addr % ROM.Length]; 
        }

        public void WriteByte(ushort addr, byte value)
        {
            throw new NotImplementedException();
        }
    }
}
