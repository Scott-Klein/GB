namespace GB.Emulator
{
    /// <summary>
    /// Represents a cart with just an MBC1.
    /// </summary>
    ///
    public class CartridgeMBC1 : CartridgeMBC
    {

        public CartridgeMBC1(string romFile) : base(romFile)
        {
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
        public override void WriteByte(ushort addr, byte value)
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
                case var a when a <= 0x3fff:
                    this.lowBank = (value & 0x1f);
                    if (this.lowBank == 0)
                    {
                        this.lowBank++;
                    }
                    break;
                //Switch the rom bank set, (1-31), (32-
                case var a when a >= 0x4000 && a <= 0x5fff:
                    this.highBank = (value & 3) << 5;
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
