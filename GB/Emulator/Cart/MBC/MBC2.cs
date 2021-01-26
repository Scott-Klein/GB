namespace GB.Emulator.Cart.MBC
{
    public class MBC2 : MBC, ICartROM
    {
        public MBC2(byte[] rom, RomInfo info) : base(rom, info)
        {
        }

        public byte ReadByte(ushort addr)
        {
            return addr switch
            {
                var a when a <= 0x3fff => ROM[addr % ROM.Length],
                var a when a <= 0x7fff => ROM[(addr * lowBank) % ROM.Length],
                var a when a >= 0xa000 && a <= 0xa1ff => RAM.ReadByte(addr % 512)
            };
        }

        public void WriteByte(ushort addr, byte value)
        {
            switch (addr)
            {
                case var a when a <= 0x3fff && (addr & 0x100) == 0: // 8 bit is clear
                    ramEnable = (value & 0xa) == 0xa ? true : false;
                    break;

                case var a when a <= 0x3fff && (addr & 0x100) == 1: // 8 bit is set
                    lowBank = (value & 0xf) != 0 ? (value & 0xf) : 1; //if bank 0 is written, write bank 1 instead.
                    break;

                case var a when a >= 0xa000 && a <= 0xa1ff:
                    RAM.Write(addr, value);
                    break;
            }
        }
    }
}