using GB.Emulator.Cart;
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
        protected readonly byte[] rom;

        private RomInfo info;

        public virtual string CartridgeOutOfRange(string addr) => $"Cartridge base class addresses up to {addr}, received address was out of bounds for a plain 32KiB cartridge.";

        public RomInfo Info { get { return this.info; } }

        //I might need a parameterless constructor for the derived classes.
        public Cartridge()
        {

        }

        public Cartridge(string romFile)
        {
            this.rom = File.ReadAllBytes(romFile);
            this.info = ReadInfo();
        }

        private RomInfo ReadInfo()
        {
            RomInfo result = new RomInfo();

            result.Name = Encoding.UTF8.GetString(this.ReadByte(AddressHelper.ROM_TITLE, GB_TITLE_LENGTH)).Trim('\0');

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
            return result;
        }

        public virtual byte ReadByte(ushort addr)
        {
            if (addr > 0x7fff)
            {
                var ex = new ArgumentOutOfRangeException("addr", addr, this.CartridgeOutOfRange(addr.ToString()));
                throw ex;
            }
            return rom[addr]; 
        }

        public virtual byte[] ReadByte(ushort addr, ushort bytesToRead)
        {
            byte[] result = new byte[bytesToRead];
            for (ushort i = 0; i < bytesToRead; i++)
            {
                result[i] = this.ReadByte((ushort)(addr + i));
            }
            return result;
        }
    }
}
