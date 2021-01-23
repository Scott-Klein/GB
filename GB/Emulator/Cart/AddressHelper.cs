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
    }
}
