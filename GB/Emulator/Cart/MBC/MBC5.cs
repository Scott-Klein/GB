namespace GB.Emulator.Cart.MBC
{
    public class MBC5 : MBC1, ICartROM
    {
        protected int RomBank16b;
        int ramBytes;
        public MBC5(byte[] rom, RomInfo info) : base(rom, info)
        {
            switch(exRam)
            {
                case ExRam.k8:
                    ramBytes = 1 << 13;
                    break;
                case ExRam.k32:
                    ramBytes = 1 << 15;
                    break;
                case ExRam.k128:
                    ramBytes = 1 << 17;
                    break;
            }
        }

        public override byte ReadByte(ushort addr)
        {
            if (addr <= 0x3fff)
            {
                return base.ReadByte(addr);
            }
            switch(addr)
            {
                case var a when a <= 0x7fff:
                    return ROM[((addr & 0x3fff) + (RomBank16b * 0x3fff)) % ROM.Length];
                case var a when a >= 0xa000 && a <= 0xbfff:
                    return RAM.ReadByte(((addr & 0x1fff) + (ramBank * 0x1fff)) % ramBytes);
                default:
                    return 0xff;
            }
        }

        public override void WriteByte(ushort addr, byte value)
        {
            if (addr <= 0x1fff) // 0000-1FFF - RAM Enable (Write Only)
            {
                base.WriteByte(addr, value);
            }
            switch (addr)
            {
                case var a when a <= 0x2fff: //2000-2FFF - Low 8 bits of ROM Bank Number (Write Only)
                    this.RomBank16b = this.RomBank16b | (value & 0xff);
                    break;
                case var a when a <= 0x3fff: //3000-3FFF - High bit of ROM Bank Number (Write Only)
                    this.RomBank16b = (value & 0x100) | (this.RomBank16b & 0xff);
                    break;
                case var a when a <= 0x5fff: //4000 - 5FFF - RAM Bank Number(Write Only)
                    this.ramBank = (value & 0xf);
                    break;
                case var a when a >= 0xa000 && a <= 0xbfff:
                    RAM.Write(((a & 0x1fff) + (ramBank * 0x1fff)) % ramBytes, value);
                    break;
                default:

                    break;
            }
        }
    }
}