namespace GB.Emulator
{
    public struct RomInfo
    {
        /// <summary>
        /// The cartridge contains the title of the game in UPPER CASE ASCII.
        /// </summary>
        public string Name;

        /// <summary>
        /// 0x014d of the cartridge contains an 8 bit checksum
        /// if the checksum failed then cartridges were not meant to run.
        /// We will not check the sum, but include it here anyway for documentation.
        /// </summary>
        public byte HeaderChecksum;

        /// <summary>
        /// Specifies the different types of Member Bank Controller that may be used in the cart, and what types of additional hardware exists as well.
        /// </summary>
        public CartridgeType Type;

        /// <summary>
        /// Roms contain an SGB flag to specify wherther the game supports any extra SGB functions.
        /// A real SGB turns of its special functions if this by is set to anything that isn't 0x03
        /// </summary>
        public bool SgbFunctions;

        /// <summary>
        /// Specifies the ROM size of the cartridge. Typically calculated as "N such that 32Kib << N"
        /// </summary>
        public RomSize Size;
        public int RomBytes;
        /// <summary>
        /// Specifies the size of the external RAM in the cartridge (if any).
        /// </summary>
        public ExRam ExternalRam;
        public int ExRamSize;
        /// <summary>
        /// Specifies if this version of the game is supposed to be sold in Japan, or anywhere else. Only two values are defined.
        /// </summary>
        public Destination Destination;

        /// <summary>
        /// The cartridge can be a CGB cart or a normal GB cart.
        /// </summary>
        public CGBcompat CGB;
    }
}

/// <summary>
/// Cartridges can contain a flag that specifies what cgb functionality it has,
/// if it is a non cgb cartridge then this space will be taken by the game title,
/// in cgb cartridges the game title is reduced to 11 bytes and this flag takes some
/// of the lost space.
/// </summary>
public enum CGBcompat : byte
{
    GB,
    Both = 0x80,
    CGB  = 0xc0,
}

public enum Destination : byte
{
    Japan = 0x00,
    Not_Japan = 0x01
}