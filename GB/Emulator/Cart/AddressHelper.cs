using System;
using System.Collections.Generic;
using System.Text;

namespace GB.Emulator.Cart
{
    /// <summary>
    /// Contains very commonly used constants to access important
    /// sections of memory.
    /// </summary>
    public static class AddressHelper 
    {
        public const int CGB_FLAG = 0x0143;

        public const int ROM_TITLE = 0x134;

        public const int ROM_SIZE = 0x148;

        public const int NINTENDO_LOGO = 0x104;

        public const int CART_TYPE = 0x147;

        public const int ENTRY = 0x100;

        public const int SGB_FLAG = 0x146;

        public const int RAM_SIZE = 0x149;

        public const int DESTINATION = 0x14a;

        public const int HEADER_CHKSM = 0x14d;

        public const int SRAM_START = 0xa000;

        public const int SRAM_END = 0xbfff;

        public const int SRAM_MASK = 0x1fff;

        public const int SRAM_BANK_WIDTH = 0x2000;

        public const int SRAM_ENABLE_START = 0x0;

        public const int SRAM_ENABLE_END = 0x1fff;

        public const int ROM_BANK_START = 0x2000;

        public const int ROM_BANK_END = 0x3fff;

        public const int ROM_RAM_BANK_START = 0x4000;

        public const int ROM_RAM_BANK_END = 0x5fff;

        public const int BANK_MODE_START = 0x6000;

        public const int BANK_MODE_END = 0x7fff;

        public const int ROM_BANK_WIDTH = 0x3fff;
    }
}
