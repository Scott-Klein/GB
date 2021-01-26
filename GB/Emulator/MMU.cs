using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator
{
    class MMU
    {
        Cartridge rom;
        PPU ppu;
        private byte[] bootRom;
        private bool bootEnable;
        private byte[] RAM;
        public MMU(Cartridge cartridge, PPU ppu)
        {
            rom = cartridge;
            this.ppu = ppu;
        }

        public byte rb(ushort addr)
        {
            if (bootEnable && addr < 0x100)
            {
                return bootRom[addr];
            }
            return addr switch
            {
                var a when a <= 0x7fff => rom.ReadByte(addr),
                var a when a <= 0x9fff => ppu.VRAM[addr & 0x1fff],
                var a when a <= 0xbfff => rom.ReadByte(addr),
                var a when a <= 0xfe00 => RAM[addr & 0x1fff],
                var a when a <= 0xfe9f => 0x0,//Graphics information.



            };
            return 0;
        }
        public byte rw(ushort addr)
        {
            return 0;
        }
        public void wb(ushort addr, byte value)
        {

        }
        public void ww(ushort addr, ushort value)
        {

        }

    }
}
