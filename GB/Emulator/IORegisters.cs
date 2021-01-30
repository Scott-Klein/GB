using System;

namespace GB.Emulator
{
    public class IORegisters
    {
        public byte Joy { get; set; }

        public void Write(int addr)
        {
            switch (addr)
            {
                //Port
                case <= 0xff02:
                    break;
                //nop
                case 0xff03:
                    break;
                //Port
                case <= 0xff07:
                    break;
                case <= 0xff09:
                    //nop
                    break;
                case <= 0xff26:
                //sound
                case <= 0xff29:
                    //nop
                    break;
                case <= 0xff3f:
                    //WaveForm Ram
                    break;
                case <= 0xff4b:
                    //LCD
                    break;
                case <= 0xff4e:
                    //nop
                    break;
                case 0xff4f:
                    //VRAM Bank select
                    break;
                case 0xff50:
                    //bootrom disable
                    break;
                case <= 0xff55:
                    //HDMA
                    break;
                case <= 0xff67:
                    //nop
                    break;
                case <= 0xff69:
                    //BCP/OCP
                    break;
                case 0xff70:
                    //WRAM Bank select
                    break;
                default:
                    throw new NotImplementedException("IO Register address out of range");
            }
        }
    }

    public enum IO
    {
        Port,
        Sound,
        WaveFormRam,
        LCD,
        VRAM_Select,
        Boot_ROM,
        HDMA,
        BCP_OCP,
        WRAM_SELECT
    }
}