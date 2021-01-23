namespace GB.Emulator
{
    public enum RomSize : byte
    {
        k32  = 0x00,
        k64  = 0x01,
        k128 = 0x02,
        k256 = 0x03,
        k512 = 0x04,
        m1   = 0x05,
        m2   = 0x06,
        m4   = 0x07,
        m8   = 0x08,
        m11  = 0x52,
        m12  = 0x53,
        m15  = 0x54
    }
}
