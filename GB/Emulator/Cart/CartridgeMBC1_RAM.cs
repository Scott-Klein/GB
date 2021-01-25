using GB.Emulator.Cart;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace GB.Emulator
{
    public class CartridgeMBC1_RAM : CartridgeMBC1
    {
        protected MemoryMappedViewAccessor ram;
        protected bool ramEnable;
        protected int ramBank;


        //Public property for unit testing.
        public bool RamEnable { get { return ramEnable; } }
        public bool CanWriteRam { get { return this.ram.CanWrite; } }
        public bool CanReadRam { get { return this.ram.CanRead; } }
        public int RAMbank { get { return this.ramBank; } }

        public CartridgeMBC1_RAM(string romFile) : base(romFile)
        {
            MemoryMappedFile mmf;
            if (this.Info.Type == CartridgeType.MBC1_RAM_BATTERY || this.Info.Type == CartridgeType.MBC3_RAM_BATTERY)
            {
                mmf = MemoryMappedFile.CreateFromFile(romFile.Remove(romFile.IndexOf('.')) + ".sav", FileMode.OpenOrCreate,null, this.Info.ExRamSize * 1024);
            }
            else
            {
                mmf = MemoryMappedFile.CreateNew(null, this.Info.ExRamSize * 1024);
            }

            this.ram = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.ReadWrite);
        }

        /// <summary>
        /// Writes bytes to the cart, knowing that ram exists.
        /// </summary>
        /// <param name="addr">The address to write to</param>
        /// <param name="value">The byte that will be written</param>
        public override void WriteByte(ushort addr, byte value)
        {
            //I need to rewrite this as a switch
            if (addr < 0x2000)
            {
                if ((value &0xf) == 0x0a)
                {
                    this.ramEnable = true;
                }
                else
                {
                    this.ramEnable = false;
                }
            }
            else if(addr >= 0x4000 && addr <= 0x5fff && this.BankMode == BankingMode.RAM)
            {
                this.ramBank = value & 0x3;
            }
            else if (addr >= AddressHelper.SRAM_START && addr <= AddressHelper.SRAM_END)
            {
                ram.Write(AddressHelper.SRAM_BANK_WIDTH * this.ramBank + (addr & AddressHelper.SRAM_MASK), value);
            }
            else
            {
                base.WriteByte(addr, value);
            }
        }

        public override byte ReadByte(ushort addr)
        {
            if (addr >= AddressHelper.SRAM_START && addr <= AddressHelper.SRAM_END)
            {
                if (this.ramEnable)
                {
                    return ram.ReadByte(AddressHelper.SRAM_BANK_WIDTH * this.ramBank + (addr & AddressHelper.SRAM_MASK));
                }
                else
                {
                    return 0xff;//Spec demands that if ram is disabled, reads return  0xff.
                }
            }
            else
            {
                return base.ReadByte(addr);
            }
            
        }

    }
}
