using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace GB.Emulator
{
    /// <summary>
    /// Represents a cart with an MBC2, this variant always has 512x4 bits Ram
    /// </summary>
    public class CartridgeMBC2 : CartridgeMBC
    {
        private const int RAM_ENABLE_MASK = 0x100;
        private const int RAM_ENABLE = 0xa;
        private bool ramEnable;
        private int ramBank;
        private MemoryMappedViewAccessor ram;
        public CartridgeMBC2(string romFile) : base(romFile)
        {
            //every mbc2 has battery backed ram, so we will create a file on the disc.
            this.ram = MemoryMappedFile.CreateFromFile(romFile.Remove(romFile.IndexOf('.')), FileMode.OpenOrCreate, null, this.Info.ExRamSize * 1024).CreateViewAccessor(0, 0, MemoryMappedFileAccess.ReadWrite);
            this.ramEnable = false;
        }

        public override byte ReadByte(ushort addr)
        {
            return addr switch
            {
                var a when a <= 0x3fff => rom[a], //0000-3FFF contains the first 16kbyte of the rom.
                var a when a <= 0x7fff => rom[((lowBank & 0xf) + (a & 0x3fff)) % Info.RomBytes],
                var a when a >= 0xa000 && a <= 0xa1ff => this.ram.ReadByte((addr & 0x1ff) % (this.Info.ExRamSize * 1024))
            };
            throw new ArgumentOutOfRangeException("addr",addr, $"Tried to read 0x{addr} from an MBC2 cartridge. This implementation of ReadByte does not handle the address inputed.");
        }


        /// <summary>
        /// The MBC2 writes are very simple, they either enable/disable ram,
        /// or they decide the rombank.
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        public override void WriteByte(ushort addr, byte value)
        {
            switch (addr)
            {
                //Ram Enable and ROM bank Number. bit 8 == 0 means we are controlling RAM
                //bit 8 == 1 means we are controlling the rom bank number.
                case var n when n <= 0x3fff && (addr & RAM_ENABLE_MASK) > 0:
                    //set the rom bank number
                    this.lowBank = value & 0xf;
                    if (this.lowBank == 0)
                    {
                        this.lowBank++;
                    }
                    break;
                case var n when n <= 0x3fff && (addr & 0x100) == 0:
                    this.ramEnable = value == RAM_ENABLE ? true : false;
                    break;
                case var n when n >= 0xa000 && n <= 0xa1ff && this.ramEnable:
                    this.ram.Write(addr & 0x1ff, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("addr", addr, $"{addr} was out of range for writing to MBC2, value: {value}");
            }
        }
    }
}