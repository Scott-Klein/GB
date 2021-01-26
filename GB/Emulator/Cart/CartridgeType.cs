namespace GB.Emulator
{
    public enum CartridgeType : byte
    {
        ROM_ONLY = 0x00,
        MBC1      = 0x01,
        MBC1_RAM = 0x02,
        MBC1_RAM_BATTERY = 0x03,
        MBC2 = 0x05,
        MBC2_BATTERY = 0x06,
        ROM_RAM = 0x08,
        ROM_RAM_BATTERY = 0x09,
        MMM01 = 0x0b,
        MMM01_RAM = 0x0c,
        MMM01_RAM_BATTERY = 0x0d,
        MBC3_TIMER_BATTERY = 0x0f,
        MBC3_TIMER_RAM_BATTERY = 0x10,
        MBC3 = 0x11,
        MBC3_RAM = 0x12,
        MBC3_RAM_BATTERY = 0x13,
        MBC5 = 0x19,
        MBC5_RAM = 0x1a,
        MBC5_RAM_BATTERY = 0x1b,
        MBC5_RUMBLE = 0x1c,
        MBC5_RUMBLE_RAM = 0x1d,
        MBC5_RUMBLE_RAM_BATTERY = 0x1e,
        MBC6 = 0x20,
        MBC7_SENSOR_RUMBLE_RAM_BATTERY = 0x22,
        POCKET_CAMERA = 0xfc,
        BANDAI_TAMA5 = 0xfd,
        HuC3 = 0xfe,
        HuC1_RAM_BATTERY = 0xff
    }

}
