using System;

namespace GB.Emulator.Cart.MBC
{
    public class MBC3 : MBC1, ICartROM
    {
        public MBC3(byte[] rom, RomInfo info) : base(rom, info)
        {
        }
        protected RAM_RTC_REG RAM_RTC_REG;
        protected RTC RTC;
        protected byte RTC_latch;
        public override byte ReadByte(ushort addr)
        {
            if (addr <= 0x7fff)
            {
                return addr switch
                {
                    var a when a <= 0x3fff => this.ROM[addr],
                    _ => this.ROM[(lowBank * 0x4000) + (addr & 0x3fff)]
                };
            }
            if (ramEnable)
            {
                return addr switch
                {
                    var a when exRam == ExRam.k32 && a >= 0xa000 && a <= 0xbfff && RAM_RTC_REG < RAM_RTC_REG.RTC_S => RAM.ReadByte(((a & 0x1fff) + ((byte)RAM_RTC_REG * 0x1fff)) % 0x8000),
                    var a when exRam == ExRam.k2 && a > 0xa000 && a < 0xa7ff => RAM.ReadByte((a & 0x7ff) % 0x7ff), //2k ram
                    var a when exRam == ExRam.k8 && a > 0xa000 && a < 0xbfff => RAM.ReadByte((a & 0x1fff) % 0x1fff), //8k, 1 bank
                    var a when a >= 0xa000 & a <= 0xbfff && RAM_RTC_REG == RAM_RTC_REG.RTC_S => RTC.RTC_S,
                    var a when a >= 0xa000 & a <= 0xbfff && RAM_RTC_REG == RAM_RTC_REG.RTC_M => RTC.RTC_M,
                    var a when a >= 0xa000 & a <= 0xbfff && RAM_RTC_REG == RAM_RTC_REG.RTC_H => RTC.RTC_H,
                    var a when a >= 0xa000 & a <= 0xbfff && RAM_RTC_REG == RAM_RTC_REG.RTC_DL => RTC.RTC_DL,
                    var a when a >= 0xa000 & a <= 0xbfff && RAM_RTC_REG == RAM_RTC_REG.RTC_DH => RTC.RTC_DH,
                };
            }
            else
            {
                return 0xff;
            }
                       
        }

        public override void WriteByte(ushort addr, byte value)
        {
            switch (addr)
            {
                case var a when a <= 0x1fff:
                    base.WriteByte(addr, value);//ramEnable behaviour is the same of mbc1
                    break;
                case var a when a <= 0x3fff:
                    this.lowBank = (value & 0x7f) > 0 ? (value & 0x7f) : 1; //bank 0x0 is mapped to the first 16k of the rom, automatically go to bank one.
                    break;
                case var a when a <= 0x5fff:
                    this.RAM_RTC_REG = (RAM_RTC_REG)value; //the enum has the bytes mapped correctly.
                    break;
                case var a when a <= 0x7fff:
                    if (value == 1 && RTC_latch == 0)
                    {
                        //Latch the RTC
                        throw new NotImplementedException("RTC Clock has not been implemented");
                    }
                    RTC_latch = value;
                    break;
                case var a when a >= 0xa000 && a <= 0xbfff:
                    switch(RAM_RTC_REG)
                    {
                        case RAM_RTC_REG.RTC_S:
                            RTC.RTC_S = (byte)(value & 0x3b);
                            break;
                        case RAM_RTC_REG.RTC_M:
                            RTC.RTC_M = (byte)(value & 0x3b);
                            break;
                        case RAM_RTC_REG.RTC_H:
                            RTC.RTC_H = (byte)(value & 0x17);
                            break;
                        case RAM_RTC_REG.RTC_DL:
                            RTC.RTC_DL = value;
                            break;
                        case RAM_RTC_REG.RTC_DH:
                            RTC.RTC_DH = (byte)(value & 0xc1);
                            break;
                        default:
                            if(ramEnable) //Might need to wrap the ram read if it's bugged.
                            {
                                RAM.Write((a & 0x1fff) + ((byte)RAM_RTC_REG * 0x1fff), value);
                            }
                            break;
                    }
                    break;
            }
        }
    }
}