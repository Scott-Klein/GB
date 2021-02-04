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
        private readonly Clock clock;
        Joypad Joy;
        private byte _IF;
        public byte IE = 0xe1; //interrupt flags initial state is e1;
        
        public byte IF { 
            get
            {
                return (byte)(0xe0 | _IF);
            }
            set
            {
                _IF = value;
            }
        }
        

        
        private byte[] bootRom;
        private bool bootEnable;
        private byte[] RAM;
        private byte[] HRAM; //zero-page ram.
        private byte[] IOregisters;

        public MMU()
        {
            InitialiseMemory();
        }
        public MMU(Cartridge cartridge, PPU ppu, Clock clock, bool testing = false, byte testInstruction = 0x0)
        {
            this.ppu = ppu;
            this.clock = clock;
            Joy = new Joypad();
            ppu.SetMMU(this);
            InitialiseMemory();
            rom = cartridge;
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
        public void Tick()
        {
            ppu.Tick();
        }
        private void InitialiseMemory()
        {
            RAM = new byte[0x2000];
            HRAM = new byte[0x80];
            IOregisters = new byte[0x80];
        }

        public byte rb(ushort addr)
        {
            if (bootEnable && addr < 0x100)
            {
                return bootRom[addr];
            }
            return addr switch
            {
                0xff0f => (byte)(0xe0 | IF),
                0xffff => IE,
                var a when a <= 0x7fff => rom.ReadByte(addr),
                var a when a <= 0x9fff => ppu.VRAM[(addr & 0x1fff) % ppu.VRAM.Length],
                var a when a <= 0xbfff => rom.ReadByte(addr),
                var a when a <= 0xfdff => RAM[addr & 0x1fff],
                var a when a <= 0xfe9f => ppu.OAM[addr & 0xff],//[FE00-FE9F] Graphics: sprite information:
                0xff50 => Convert.ToByte(bootEnable),
                var a when a >= 0xff40 && a <= 0xff4b => ppu.ReadByte(addr),
                var a when a >= 0xff04 && a <= 0xff07 => clock.ReadByte(addr),
                var a when a >= 0xff80 && a <= 0xfffe => HRAM[0x7f & addr],
                var a when a >= 0xff00 && a <= 0xff7f => this.IOregisters[0xff & a],
                var a when a >= 0xfea0 && a <= 0xfeff => 0xff
            };
            throw new NotImplementedException();
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
                case <= 0x7fff:
                    rom.WriteByte(addr, value);
                    break;
                case 0xff0f:
                    IF = value;
                    break;
                case 0xffff:
                    IE = value;
                    break;
                case var a when a >= 0xa000 && a <= 0xbfff:
                    //Cart ram?
                    rom.WriteByte(addr, value);
                    break;
                case var a when a >= 0xc000 && a <= 0xdfff:
                    RAM[addr & 0x1fff] = value;
                    break;
                case var a when a >= 0xfe00 && a <= 0xfe9f:
                    ppu.WriteByte(addr, value);
                    break;
                case var a when a >= 0xff40 && a <= 0xff4b:
                    ppu.WriteByte(addr, value);
                    break;
                case 0xff50:
                    bootEnable = value < 0;
                    break;
                case var a when a >= 0x8000 && a <= 0x9fff:
                    ppu.WriteByte(addr, value);
                    break;
                case var a when a >= 0xff80 && a <= 0xfffe:
                    HRAM[0x7f & addr] = value;
                    break;
                case var a when a >= 0xff04 && a <= 0xff07:
                    clock.WriteByte(a, value);
                    break;
                case var a when a >= 0xff00 && a <= 0xff7f:
                    IOregisters[addr & 0x00ff] = value;
                    break;
                case var a when a >= 0xfea0 && a <= 0xfeff:
                    break;
                default:
                    throw new NotImplementedException($"{addr:X2}  :  Address isn't able to be written to.");
            }
        }
        public void WriteWord(int addr, ushort value)
        {
            wb((ushort)(addr + 1), (byte)(value >> 8)); //msb goes one address higher
            wb((ushort)(addr), (byte)(0x00ff & value)); //lsb goes to the address.
        }

    }
}
