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
}
