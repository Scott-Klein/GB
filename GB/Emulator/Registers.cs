using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator
{
    public interface IRegisters
    {
        ushort AF { get; set; }
        ushort BC { get; set; }
        ushort DE { get; set; }
        ushort HL { get; set; }
        bool Zero { get; set; }
        bool Carry { get; set; }
        bool Subtract { get; set; }
        bool HalfCarry { get; set; }

        public byte A { get; set; }
        public byte B { get; set; }
        public byte C { get; set; }
        public byte D { get; set; }
        public byte E { get; set; }
        public byte H { get; set; }
        public byte L { get; set; }
    }

    public class Registers : IRegisters
    {
        public Registers(bool SkipBoot = false)
        {
            if (SkipBoot)
            {
                //Set default registers
                AF = 0x1b0;
                BC = 0x13;
                DE = 0xD8;
                HL = 0x14d;
                sp = 0xfffe;
                pc = 0x100;
            }
        }

        public byte a;
        public byte b;
        public byte c;
        public byte d;
        public byte e;
        public byte h;
        public byte l;
        public ushort sp;
        public ushort pc;
        public byte f;

        public ushort AF
        {
            get
            {
                return Combine(a, f);
            }
            set
            {
                this.a = (byte)(value >> 8);
                this.f = (byte)(value & 0xff);
            }
        }

        public ushort BC
        {
            get
            {
                return Combine(b, c);
            }
            set
            {
                this.b = (byte)(value >> 8);
                this.c = (byte)(value & 0xff);
            }
        }

        public ushort DE
        {
            get
            {
                return Combine(d, e);
            }
            set
            {
                this.d = (byte)(value >> 8);
                this.e = (byte)(value & 0xff);
            }
        }

        public ushort HL
        {
            get
            {
                return Combine(h, l);
            }
            set
            {
                this.h = (byte)(value >> 8);
                this.l = (byte)(value & 0xff);
            }
        }

        public bool Zero { get; set; }
        public bool Carry { get; set; }
        public bool Subtract { get; set; }
        public bool HalfCarry { get; set; }


        private static ushort Combine(byte hi, byte lo)
        {
            return (ushort)((hi << 8) | lo);
        }
    }
}
