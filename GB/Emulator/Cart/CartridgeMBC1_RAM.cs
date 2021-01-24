using GB.Emulator.Cart;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace GB.Emulator
{
    public class CartridgeMBC1_RAM : CartridgeMBC1
    {
        private MemoryMappedViewAccessor ram;
        private bool ramEnable;
        private int ramBank;


        //Public property for unit testing.
        public bool RamEnable { get { return ramEnable; } }
        public bool CanWriteRam { get { return this.ram.CanWrite; } }
        public bool CanReadRam { get { return this.ram.CanRead; } }
        public int RAMbank { get { return this.ramBank; } }

        public CartridgeMBC1_RAM(string romFile) : base(romFile)
        {
            int ramSize;
            switch (this.Info.ExternalRam)
            {
                case ExRam.k2:
                    ramSize = 2;
                    break;
                case ExRam.k8:
                    ramSize = 8;
                    break;
                case ExRam.k32:
                    ramSize = 32;
                    break;
                case ExRam.k128:
                    ramSize = 128;
                    break;
                case ExRam.k64:
                    ramSize = 64;
                    break;
                default:
                    ramSize = 0;
                    break;
            }
            MemoryMappedFile mmf;
            if (this.Info.Type == CartridgeType.MBC1_RAM_BATTERY)
            {
                mmf = MemoryMappedFile.CreateFromFile(romFile.Remove(romFile.IndexOf('.')) + ".sav", FileMode.OpenOrCreate,null, ramSize * 1024);
            }
            else
            {
                mmf = MemoryMappedFile.CreateNew(null, ramSize * 1024);
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
            if (addr < 0x2000)
            {
                if (value == 0x0a)
                {
                    this.ramEnable = true;
                }
                else
                {
                    this.ramEnable = false;
                }
            }
            if (addr >= 0x4000 && addr <= 0x5fff && this.BankMode == BankingMode.RAM)
            {
                this.ramBank = value & 0x3;
            }
            if (addr >= AddressHelper.SRAM_START && addr <= AddressHelper.SRAM_END)
            {
                
                ram.Write(AddressHelper.SRAM_BANK_WIDTH * this.ramBank + (addr & AddressHelper.SRAM_MASK), value);
            }
            base.WriteByte(addr, value);
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
