using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator.Cart.MBC
{
    public struct RTC
    {
        public byte RTC_S;
        public byte RTC_M;
        public byte RTC_H;
        public byte RTC_DL;
        public byte RTC_DH;
    }
    public enum RAM_RTC_REG : byte
    {
        r0 = 0x0,
        r1 = 0x1,
        r2 = 0x2,
        r3 = 0x3,
        r4 = 0x4,
        r5 = 0x5,
        r6 = 0x6,
        r7 = 0x7,
        RTC_S = 0x8,
        RTC_M = 0x9,
        RTC_H = 0xa,
        RTC_DL = 0xb,
        RTC_DH = 0xc
    }
}
