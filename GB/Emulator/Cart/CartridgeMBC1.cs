using GB.Emulator.Cart;

namespace GB.Emulator
{

    public class CartridgeMBC : Cartridge
    {
        protected int bank;
        protected int bankSet;
        protected BankingMode bankMode;

        /* Props for unit testing */
        public BankingMode BankMode { get { return this.bankMode; } }
        public int Bank { get { return this.bank; } }
        public int BankSet { get { return this.bankSet; } }

        public CartridgeMBC(string romFile) : base(romFile)
        {

        }
    }
    /// <summary>
    /// Represents a cart with just an MBC1.
    /// </summary>
    ///
    public class CartridgeMBC1 : CartridgeMBC
    {

        public CartridgeMBC1(string romFile) : base(romFile)
        {
        }

        public override byte ReadByte(ushort addr)
        {
            return rom[(this.bank * this.bankSet) * AddressHelper.ROM_BANK_WIDTH + addr];
        }

        /// <summary>
        /// Writes to the MBC for the purpose of controlling the registers
        /// that influence rom bank selection.
        /// </summary>
        /// <param name="addr">
        /// 16 bit address used to write to the rom.
        /// </param>
        /// <param name="value">
        /// a byte to write to the rom at the location, specific bytes control
        /// enabling and disabling ram, and selecting rom banks.
        /// </param>
        public virtual void WriteByte(ushort addr, byte value)
        {
            switch (addr)
            {
                //ROM bank register, switch between banks 1-31
                case var a when a <= 0x1fff:
                    /*
                     * do nothing as this is the MBC1 base class, 
                     * overrides for ram implementations will handle these cases.
                    */
                    break;
                case var a when a > 0x1fff && a <= 0x3fff:
                    this.bank = (value & 0x1f);
                    if (this.bank == 0)
                    {
                        this.bank++;
                    }
                    break;
                //Switch the rom bank set, (1-31), (32-
                case var a when a >= 0x4000 && a <= 0x5fff:
                    this.bankSet = (value & 0x18) >> 3;
                    break;
                case var a when a <= 0x7fff:
                    if ((value & 1) == 1)
                    {
                        bankMode = BankingMode.RAM;
                    }
                    else
                    {
                        bankMode = BankingMode.ROM;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Represents a cart with an MBC2, this variant always has 512x4 bits Ram
    /// </summary>
    public class CartridgeMBC2 : CartridgeMBC
    {
        public CartridgeMBC2(string romFile) : base(romFile)
        {

        }
    }
}
