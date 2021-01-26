using GB.Emulator.Cart;
using GB.Emulator.Cart.MBC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GB.Emulator
{
    public class Cartridge
    {
        public const int GB_TITLE_LENGTH = 16; //early carts had 16 bytes for the title
        public const int GBC_TITLE_LENGTH = 11; //later carts used only 11 bytes.

        private RomInfo info;

        private ICartROM ROM;

        public RomInfo Info { get { return this.info; } }

        public Cartridge(string romFile)
        {
            var rom = File.ReadAllBytes(romFile);
            this.ROM = new StandardROM(rom);//this will be discarded if the header says a different type.
            this.info = ReadInfo();

            this.info.FileName = romFile;

            switch (this.info.Type)
            {
                case CartridgeType.ROM_ONLY:
                    this.ROM = new StandardROM(rom);
                    break;
                case var t when (byte)t <= 0x03:
                    this.ROM = new MBC1(rom, this.info);
                    break;
                case var t when (byte)t == 0x05 || (byte)t == 0x06:
                    this.ROM = new MBC2(rom, this.info);
                    break;
                case var t when (byte)t >= 0xf && (byte)t <= 0x13:
                    this.ROM = new MBC3(rom, this.info);
                    break;
                case var t when (byte)t >= 0x19 && (byte)t <= 0x1e:
                    this.ROM = new MBC5(rom, this.info);
                    break;
                case CartridgeType.HuC1_RAM_BATTERY:
                    this.ROM = new Huc1(rom, this.info);
                    break;
                default:
                    throw new NotImplementedException($"Rom type {this.info.Type} has not been implemented in this emulator");
            }
        }

        private RomInfo ReadInfo()
        {
            RomInfo result = new RomInfo();
            
            result.Name = Encoding.UTF8.GetString(this.ReadBytes(AddressHelper.ROM_TITLE, GB_TITLE_LENGTH)).Trim('\0');

            //Read the cgb flag. 
            if (result.Name.Length == 11)
            {
                result.CGB = (CGBcompat)this.ReadByte(AddressHelper.CGB_FLAG);
            }
            else
            {
                result.CGB = CGBcompat.GB;
            }
            result.Size = (RomSize)this.ReadByte(AddressHelper.ROM_SIZE);
            result.Type = (CartridgeType)this.ReadByte(AddressHelper.CART_TYPE);

            
            result.Destination = (Destination)this.ReadByte(AddressHelper.DESTINATION);
            result.ExternalRam = (ExRam)this.ReadByte(AddressHelper.RAM_SIZE);
            int ramSize;
            switch (result.ExternalRam)
            {
                case ExRam.k2:
                    ramSize = 1 << 12;
                    break;
                case ExRam.k8:
                    ramSize = 1 << 13;
                    break;
                case ExRam.k32:
                    ramSize = 1 << 15;
                    break;
                case ExRam.k128:
                    ramSize = 1 << 17;
                    break;
                case ExRam.k64:
                    ramSize = 1 << 16;
                    break;
                default:
                    ramSize = 0;
                    break;
            }
            result.ExRamSize = ramSize;

            switch (result.Size)
            {
                case RomSize.k32:
                    result.RomBytes = 2 << 15;
                    break;
                case RomSize.k64:
                    result.RomBytes = 2 << 16;
                    break;
                case RomSize.k128:
                    result.RomBytes = 2 << 17;
                    break;
                case RomSize.k256:
                    result.RomBytes = 2 << 18;
                    break;
                case RomSize.k512:
                    result.RomBytes = 2 << 19;
                    break;
                case RomSize.m1:
                    result.RomBytes = 2 << 20;
                    break;
                case RomSize.m2:
                    result.RomBytes = 2 << 21;
                    break;
                case RomSize.m4:
                    result.RomBytes = 2 << 22;
                    break;
                case RomSize.m8:
                    result.RomBytes = 2 << 23;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return result;
        }

        public virtual byte ReadByte(ushort addr)
        {
            return this.ROM.ReadByte(addr);
        }

        public byte[] ReadBytes(ushort addr, ushort bytesToRead)
        {
            var result = new byte[bytesToRead];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ReadByte((ushort)(addr + i));
            }
            return result;
        }

        public void WriteByte(ushort addr, byte value)
        {
            this.ROM.WriteByte(addr, value);
        }
    }
}
