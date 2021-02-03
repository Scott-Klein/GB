using System;
using GB.Emulator.Video;

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
        public bool BGTileMap_3
        {
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
        private byte _value = 0x84;
        public byte Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }


        public bool LYC_Compare_Enable_6
        {
            get
            {
                return (_value & 0x40) > 0;
            }
            set
            {
                throw new AccessViolationException("LYC is set via bytes");
            }
        }
        public bool Mode2OAMcheckEnable_5
        {
            get
            {
                return (_value & 0x20) > 0;
            }
            set
            {
                throw new AccessViolationException("OAM Enable is set via write bytes");
            }
        }

        public bool Mode1VBlankcheckEnable_4
        {
            get
            {
                return (_value & 0x10) > 0;
            }
            set
            {
                throw new AccessViolationException("VBlank check enable is written only via bytes");
            }
        }

        public bool Mode0HBlankCheckEnable_3
        {
            get
            {
                return (_value & 0x8) > 0;
            }
            set
            {
                throw new AccessViolationException("Hblank check enable is written only via bytes");
            }
        }

        public bool LY_Comparison_Signal
        {
            get
            {
                return (_value & 0x4) > 0;
            }
            set
            {
                if (value)
                {
                    _value |= 0x4;
                }
                else
                {
                    _value &= 0xfb;
                }
            }
        }

        public ScreenMode ScreenMode { get; set; }
    }
    public class PPU
    {
        private bool vBlank;
        public bool V_BLANK
        {
            get
            {
                return vBlank;
            }
        }

        private LCDCRegisters LCDC;
        private STATRegisters STAT;
        private long totalCycles;
        private const int VRAM_START = 0x8000;
        private const int VRAM_END = 0x9fff;
        private const int VRAM_SIZE = 0x2000;
        private const int OAM_START = 0xfe00;
        private const int OAM_END = 0xfe9f;
        private const int OAM_SIZE = 0xa0;
        private const int GB_WIDTH = 160;
        private const int GB_HEIGHT = 144;
        private readonly Clock clock;

        public byte[] VRAM { get; }
        public byte[] OAM { get; }
        private bool startup = true;
        private byte lcdc;
        private byte SCX;
        private byte ScanLineC; // Causes an interrupt when ly and lyc match.
        private byte stat;
        public Render Renderer;
        private byte scanLine;
        public byte Scanline
        {
            get
            {
                return LCDC.LCD_7 ? this.scanLine : 0x0; //if the lcd is off return 0x0
            }
            set
            {
                if (value == ScanLineC) // on increment only
                {
                    STAT.LY_Comparison_Signal = true;
                    ModeUpdate(STAT.ScreenMode);
                }
                this.scanLine = value;
            }
        }

        private int Cycle;


        public PPU(Clock clock)
        {
            VRAM = new byte[VRAM_SIZE];
            OAM = new byte[OAM_SIZE];
            LCDC = new LCDCRegisters();
            STAT = new STATRegisters();
            Renderer = new Render(this, LCDC, VRAM);
            totalCycles = 0;
            this.clock = clock;
        }

        public void Tick()
        {
            //catch up with the cpu.
            while (this.totalCycles < clock.Cycles)
            {
                if (!LCDC.LCD_7 || startup)
                {
                    return;
                }
                this.totalCycles += 4;
                Cycle += 4;

                if (Scanline <= 143)
                {
                    //handle mode
                    L143();
                }
                else if (Scanline < 153)
                {
                    VBlank();
                }
                else if (Scanline == 153)
                {
                    Cycle = 0;
                    ModeUpdate(ScreenMode.SearchOAMRAM);
                    Scanline = 0;
                    vBlank = false;
                }
            }
        }
        private void VBlank()
        {
            if (Cycle == 4)
            {
                if (Scanline == 144)
                {
                    //ModeUpdate(ScreenMode.VBlank);
                    mmu.IF |= 0x1;
                    vBlank = true;
                }
            }
            else if (Cycle == 456)
            {
                Scanline++;
                Cycle = 0;
            }
        }

        private void L143()
        {
            switch (Cycle)
            {
                case 80:
                    ModeUpdate(ScreenMode.Transferring);
                    break;
                case 252:
                    ModeUpdate(ScreenMode.HBlank);
                    Renderer.RenderLine(Scanline);
                    break;
                case 456:
                    Cycle = 0;
                    Scanline++;
                    STAT.LY_Comparison_Signal = false;

                    if (Scanline == 144)
                        ModeUpdate(ScreenMode.VBlank);
                    else
                        ModeUpdate(ScreenMode.SearchOAMRAM);
                    break;
            }
        }

        private void ModeUpdate(ScreenMode mode)
        {
            STAT.ScreenMode = mode;

            bool interrupt = false;

            //Let the game framework know that vblank is off.
            this.vBlank = false;
            if (Scanline == ScanLineC && STAT.LYC_Compare_Enable_6)
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
            if (mode == ScreenMode.VBlank && STAT.Mode1VBlankcheckEnable_4)
            {
                interrupt = true;
            }
            if (interrupt)
            {
                mmu.IF |= 0x2;
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
                case 0xff47:
                    BGP = value;
                    break;
                case 0xff41:
                    STAT.Value = value;
                    break;
                case 0xff42:
                    Renderer.SCY = value;
                    break;
                case 0xff43:
                    Renderer.SCX = value;
                    break;
                case 0xff4b:
                    Renderer.WX = value;
                    break;
                case 0xff4a:
                    Renderer.WY = value;
                    break;
                case 0xff48:
                    Renderer.BP0 = value;
                    break;
                case 0xff49:
                    Renderer.BP1 = value;
                    break;
                default:
                    throw new NotImplementedException("Can't right to the address yet");
                    break;
            }
        }

        private byte BGP;
        public byte ReadByte(int addr)
        {
            return (byte)this.ReadByte((ushort)addr);
        }
        public byte ReadByte(ushort addr)
        {
            return addr switch
            {
                var a when a >= VRAM_START && a <= VRAM_END => VRAM[addr & VRAM_SIZE],
                var a when a >= OAM_START && a <= OAM_END => OAM[addr & OAM_SIZE],
                0xff41 => stat,
                0xff44 => Scanline,
                0xff45 => ScanLineC,
                0xff47 => BGP,
                _ => 0xff
            };
        }

        private MMU mmu;

        internal void SetMMU(MMU mMU)
        {
            this.mmu = mMU;
            this.startup = false;
        }
    }
}