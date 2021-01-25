using GB.Emulator.Cart;

namespace GB.Emulator
{
    public class CartridgeMBC : Cartridge
    {
        protected int lowBank;
        protected int highBank;
        protected BankingMode bankMode;

        /* Props for unit testing */
        public BankingMode BankMode { get { return this.bankMode; } }
        public int Bank { get { 
                return this.lowBank | this.highBank; } }

        public CartridgeMBC(string romFile) : base(romFile)
        {
            this.highBank = 0;
        }

        public virtual void WriteByte(ushort addr, byte value)
        {

        }

        public override byte ReadByte(ushort addr)
        {
            return addr switch
            {
                var a when a <= 0x3fff && bankMode == BankingMode.ROM => rom[a],
                var a when a <= 0x3fff => rom[highBank * 0x4000 + a],
                var a when a <= 0x7fff => rom[((highBank << 5) | lowBank) + (a & 0x3fff)]
            };
            
        }
    }

    public class CartridgeMBC3 : CartridgeMBC1_RAM
    {
        
        public CartridgeMBC3(string romFile) :base(romFile)
        {

        }

        public override byte ReadByte(ushort addr)
        {
            if (addr <= 0x3fff) // Pan-docs state that the behaviour is the same as MBC1, so delegate that to the base class version
            {
                return base.ReadByte(addr);
            }

            return addr switch
            {
                var a when a <= 0x7fff => rom[((highBank << 5) | lowBank) + (a & 0x3fff)]
            };
        }

        public override void WriteByte(ushort addr, byte value)
        {
            switch (addr)
            {
                case var a when a < 0x2000:
                    base.WriteByte(addr, value);
                    break;
                case var a when a < 0x4000:
                    this.ramBank = value & 0x7f;
                    break;
            }
            base.WriteByte(addr, value);
        }
    }
}
