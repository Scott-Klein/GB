namespace GB.Emulator
{
    public class PPU
    {
        private const int VRAM_START = 0x8000;
        private const int VRAM_END = 0x9fff;
        private const int VRAM_SIZE = 0x1fff;
        private const int OAM_START = 0xfe00;
        private const int OAM_END = 0xfe9f;
        private const int OAM_SIZE = 0x9f;
        private const int GB_WIDTH = 160;
        private const int GB_HEIGHT = 144;
        public byte[] VRAM { get; }
        public byte[] OAM { get; }
        private byte lcdc;
        private byte ly; // LCD Current Scanline
        private byte lyc; // Causes an interrupt when ly and lyc match.
        private byte stat;

        public int[] Pixels
        {
            get
            {
                return this.pixels;
            }
        }

        private int[] pixels;

        public PPU()
        {
            VRAM = new byte[VRAM_SIZE];
            OAM = new byte[OAM_SIZE];
            this.pixels = new int[GB_WIDTH * GB_HEIGHT];
        }

        internal void WriteByte(ushort addr, byte value)
        {
            switch (addr)
            {
                case var a when a >= VRAM_START && a <= VRAM_END:
                    VRAM[addr & VRAM_SIZE-1] = value;
                    break;

                case var a when a >= OAM_START && a <= VRAM_END:
                    OAM[addr & OAM_SIZE-1] = value;
                    break;

                case 0xff40:
                    lcdc = value;
                    break;

                default:
                    break;
            }
        }

        public byte ReadByte(ushort addr)
        {
            return addr switch
            {
                var a when a >= VRAM_START && a <= VRAM_END => VRAM[addr & VRAM_SIZE],
                var a when a >= OAM_START && a <= OAM_END => OAM[addr & OAM_SIZE],
                var a when a == 0xff44 => ly,
                var a when a == 0xff45 => lyc,
                var a when a == 0xff41 => stat,
            };
            return 0xff;
        }


    }
}