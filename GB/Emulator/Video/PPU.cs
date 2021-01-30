using System;

namespace GB.Emulator
{
    public class LCDCRegisters
    {
        public byte Value { get; set; }

        public bool LCD_7
        {
            get
            {
               return (Value & 0x80) > 1;
            }
        }

        public bool WindowTileMap_6
        {
            get
            {
                return (Value & 0x40) > 0;
            }
        }
        public bool WindowEnable_5
        {
            get
            {
                return (Value & 0x20) > 0;
            }
        }
        public bool BGWindowTileSet_4
        {
            get
            {
                return (Value & 0x10) > 0;
            }
        }
        public bool BGTileMap_3 {
            get
            {
                return (Value & 0x8) > 0;
            }
        }
        public bool SpriteSize_2
        {
            get
            {
                return (Value & 0x4) > 0;
            }
        }
        public bool SpritesEnabled_1
        {
            get
            {
                return (Value & 0x2) > 0;
            }
        }
        public bool BGEnabled_0
        {
            get
            {
                return (Value & 0x1) > 0;
            }
        }
    }

    public enum ScreenMode
    {
        HBlank,
        VBlank,
        SearchOAMRAM,
        Transferring
    }
    public class STATRegisters
    {
        public byte Value { get; set; }

        public bool LYC_LY_Check_6 { get; set; }
        public bool Mode2OAMcheckEnable_5 { get; set; }
        public bool Mode1VBlankcheckEnable_4 { get; set; }
        public bool Mode0HBlankCheckEnable_3 { get; set; }
        public bool LY_LYC_Signal { get; set; }

        public ScreenMode ScreenMode { get; set; }
    }
    public class PPU
    {
        private LCDCRegisters LCDC;
        private STATRegisters STAT;
        private const int VRAM_START = 0x8000;
        private const int VRAM_END = 0x9fff;
        private const int VRAM_SIZE = 0x2000;
        private const int OAM_START = 0xfe00;
        private const int OAM_END = 0xfe9f;
        private const int OAM_SIZE = 0xa0;
        private const int GB_WIDTH = 160;
        private const int GB_HEIGHT = 144;
        public byte[] VRAM { get; }
        public byte[] OAM { get; }
        private byte lcdc;
        private byte SCX;
        private byte ScanLineC; // Causes an interrupt when ly and lyc match.
        private byte stat;

        public int[] Pixels
        {
            get
            {
                return this.pixels;
            }
        }
        private int Cycle;
        private int[] pixels;

        public PPU()
        {
            VRAM = new byte[VRAM_SIZE];
            OAM = new byte[OAM_SIZE];
            this.pixels = new int[GB_WIDTH * GB_HEIGHT];
            LCDC = new LCDCRegisters();
            STAT = new STATRegisters();
        }

        public void Tick()
        {
            if (!LCDC.LCD_7)
            {
                return;
            }
            Cycle += 4;

            if (Scanline <= 143)
            {
                //handle mode
                L143();
            }
            else if (Scanline != 153)// Modes are a little different here
            {
                VBlank();

            }
            else
            {
                L153();
            }

        }
        private void VBlank()
        {
            if (Cycle == 4)
            {
                if (Scanline == 144)
                {
                    //ask for VBLANK INTERRUPT
                    mmu.IF |= 0x1;
                    STAT.ScreenMode = ScreenMode.VBlank;
                }

                STAT.LY_LYC_Signal = Scanline == ScanLineC;
            }
        }
        private void L153()
        {

        }
        private void L143()
        {
            switch (Cycle)
            {
                case 80:
                    STAT.ScreenMode = ScreenMode.Transferring;
                    break;
                case var c when c == (252 + ((SCX + 3) & -4)):
                    STAT.ScreenMode = ScreenMode.HBlank;
                    break;
                case 456:
                    Cycle = 0;
                    Scanline++;
                    STAT.LY_LYC_Signal = false;

                    if (Scanline == 144)
                        STAT.ScreenMode = ScreenMode.VBlank;
                    else
                        STAT.ScreenMode = ScreenMode.SearchOAMRAM;
                    break;
            }
            if (Cycle == 80)
            {
                STAT.ScreenMode = ScreenMode.Transferring;
            }
            else if (Cycle == 252 + ((SCX + 3) & -4))
            {
                STAT.ScreenMode = ScreenMode.HBlank;
            }
            else if (Cycle == 456)
            {
                Cycle = 0;
                Scanline++;
                STAT.LY_LYC_Signal = false;

                STAT.ScreenMode = ScreenMode.SearchOAMRAM;

            }
        }
        private void ModeUpdate(ScreenMode mode)
        {
            STAT.ScreenMode = mode;
            bool interrupt = false;
            if (Scanline == ScanLineC && STAT.LYC_LY_Check_6)
            {
                interrupt = true;
            }
            if (STAT.Mode0HBlankCheckEnable_3 && mode == ScreenMode.HBlank)
            {
                interrupt = true;
            }
            if (mode == ScreenMode.SearchOAMRAM && STAT.Mode2OAMcheckEnable_5)
            {
                interrupt = true;
            }
            if (mode == ScreenMode.VBlank && (STAT.Mode1VBlankcheckEnable_4 || STAT.Mode2OAMcheckEnable_5))
            {
                interrupt = true;
            }
            if (interrupt)
            {
                mmu.IF |= 0x2;
            }
        }
        private byte scanLine;
        public byte Scanline
        {
            get
            {
                return LCDC.LCD_7 ? this.scanLine : 0x0; //if the lcd is off return 0x0
            }
            set
            {
                this.scanLine = value;
            }
        }
        internal void WriteByte(ushort addr, byte value)
        {
            switch (addr)
            {
                case var a when a >= VRAM_START && a <= VRAM_END:
                    VRAM[addr - VRAM_START] = value;
                    break;

                case var a when a >= OAM_START && a <= OAM_END:
                    OAM[addr - OAM_START] = value;
                    break;

                case 0xff40:
                    LCDC.Value = value;
                    break;
                case 0xff44:
                    break;
                default:
                    throw new NotImplementedException("Can't right to the address yet");
                    break;
            }
        }

        public byte ReadByte(ushort addr)
        {
            return addr switch
            {
                var a when a >= VRAM_START && a <= VRAM_END => VRAM[addr & VRAM_SIZE],
                var a when a >= OAM_START && a <= OAM_END => OAM[addr & OAM_SIZE],
                var a when a == 0xff44 => Scanline,
                var a when a == 0xff45 => ScanLineC,
                var a when a == 0xff41 => stat,
                _=> 0xff
            };
        }

        private MMU mmu;
        internal void SetMMU(MMU mMU)
        {
            this.mmu = mMU;
        }
    }
}