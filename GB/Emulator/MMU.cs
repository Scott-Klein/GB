using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator
{
    public class MMU
    {
        Cartridge rom;
        PPU ppu;
        Joypad Joy;
        Timer timer;
        private byte[] bootRom;
        private bool bootEnable;
        private byte[] RAM;
        private byte[] HRAM; //zero-page ram.
        public MMU(Cartridge cartridge, PPU ppu, bool testing = false, byte testInstruction = 0x0)
        {
            RAM = new byte[0x1fff];
            HRAM = new byte[0x7f];
            rom = cartridge;
            this.ppu = ppu;
            

            try
            {
                if (!testing)
                {
                    this.bootRom = File.ReadAllBytes("DMG_ROM.bin");
                }
                else
                {
                    this.bootRom = new byte[1];
                    this.bootRom[0] = testInstruction;
                }
                bootEnable = true;
            }
            catch (Exception)
            {
                bootEnable = false;
            }
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
                var a when a <= 0x9fff => ppu.VRAM[(addr & 0x1fff) % ppu.VRAM.Length],
                var a when a <= 0xbfff => rom.ReadByte(addr),
                var a when a <= 0xfdff => RAM[addr & 0x1fff],
                var a when a <= 0xfe9f => ppu.OAM[addr & 0xff],//[FE00-FE9F] Graphics: sprite information: 
                var a when a == 0xff00 => Joy.P1,
                var a when a >= 0xff04 && a <= 0xff07 => timer.ReadByte(addr),
                var a when a >= 0xff80 && a <= 0xffff => HRAM[0x7f & addr]
            };
            return 0;
        }
        public byte rb(int addr)
        {
            return this.rb((ushort)addr);
        }

        public ushort rw(ushort addr)
        {
            return (ushort)((rb(addr + 1) << 8) | rb(addr));
        }

        public ushort rw(int addr)
        {
            return rw((ushort)addr);
        }


        public void wb(ushort addr, byte value)
        {
            switch(addr)
            {
                case var a when a >= 0xfe00 && a <= 0xfe9f:
                    ppu.WriteByte(addr, value);
                    break;
                case 0xff40:
                case 0xff41:
                case 0xff45:
                    ppu.WriteByte(addr, value);
                    break;
                case 0xff00:
                    Joy.P1 = value; ;
                    break;
                case 0xff50:
                    bootEnable = false;
                    break;
                case var a when a >= 0x8000 && a <= 0x9fff:
                    ppu.WriteByte(addr, value);
                    break;
                default:
                    throw new NotImplementedException($"{addr}  :  Address isn't able to be written to.");
            }
        }
        public void WriteWord(int addr, ushort value)
        {
            throw new NotImplementedException();
        }

    }
}
