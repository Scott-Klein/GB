using System;


namespace GB.Emulator.Cart.MBC
{
    public class Huc1 : MBC, ICartROM
    {
        bool IRmode;
        byte romBank;
        public Huc1(byte[] rom, RomInfo info) : base(rom, info)
        {

        }
        public byte ReadByte(ushort addr)
        {
            throw new NotImplementedException();
        }

        public void WriteByte(ushort addr, byte value)
        {
            switch (addr)
            {
                case var a when a < 0x1fff:
                    IRmode = (value & 0xe) == 0x3 ? true : false;
                    break;
                case var a when a < 0x3fff: //2000-3FFF ROM Bank Number (Write Only)
                    romBank = value;
                    break;
                case var a when a < 0x5fff:    // 4000-5FFF RAM Bank Select (Write Only)
                    ramBank = value;
                    break;
                case var a when a >= 0xa000 && a <= 0xbfff: // A000-BFFF Cart RAM or IR register (Read/Write)
                    if (!IRmode)
                    {
                        RAM.Write(((a & 0x1fff) + (ramBank * 0x1fff)) % 0x7fff, value);
                    }
                    break;
            }
        }
    }
}