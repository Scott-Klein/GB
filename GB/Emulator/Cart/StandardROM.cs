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
            //Nothing is supposed to happen in this function, but it needs to exist.
            //Some roms such as Tetris attempt to write to the MBC, this is theorised
            //to be because Tetris may have been developed on an early MBC1 cartridge.
        }
    }
}
