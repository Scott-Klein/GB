using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator
{
    public sealed class CPU
    {
        private byte a;
        private byte b;
        private byte c;
        private byte d;
        private byte e;
        private byte h;
        private byte l;
        private ushort sp;
        private ushort pc;
        private byte f;

        public ushort AF {
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
        private int m;
        private int t;

        private static ushort Combine(byte hi, byte lo)
        {
            return (ushort)((hi << 8) & lo);
        }

        public byte A {
            get
            {
                return this.a;
            }
        }
        public byte B
        {
            get
            {
                return this.b;
            }
        }
        public byte C
        {
            get
            {
                return this.c;
            }
        }
        public byte D
        {
            get
            {
                return this.d;
            }
        }
        public byte E
        {
            get
            {
                return this.e;
            }
        }
        public byte H
        {
            get
            {
                return this.h;
            }
        }
        public byte L
        {
            get
            {
                return this.l;
            }
        }
        public byte F
        {
            get
            {
                return this.f;
            }
        }
        private Clock clock;
        private MMU mmu;
        public CPU()
        {
            a = 0;
            b = 0;
            c = 0;
            d = 0;
            e = 0;
            f = 0;
            h = 0;
            l = 0;
            sp = 0;
            pc = 0;
            clock = new Clock();
            mmu = new MMU();
        }

        public void Tick()
        {
            //fetch;
            var op = mmu.rb(pc++);
            Dispatch(op);
            pc &= 0xffff; //mask the pc.
            clock.M += m;
            clock.T += t;

        }

        private void Dispatch(byte op)
        {
            switch(op)
            {
                default:
                    throw new NotImplementedException($"The op code {op} has not been implemented yet.");
            }
        }
    }
}
