using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator
{
    public sealed class CPU
    {
        private const int X_MASK = 0x3f;
        private const int Z_MASK = 0x7;
        private const int Y_MASK = 0x38;
        private const int P_MASK = 0x30;
        private const int Q_MASK = 0x8;
        private const int X_3 = 0xc0;
        private const int X_2 = 0x80;
        private const int X_1 = 0x40;
        private const int Y_1 = 0x8;
        private const int Y_2 = 0x10;
        private const int Y_3 = 0x18;
        private const int Y_4 = 0x20;
        private const int Y_5 = 0x28;
        private const int Y_6 = 0x30;
        private const int Y_7 = 0x38;


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

        public CPU(MMU mmu)
        {
            ResetRegisters();
            clock = new Clock();
            this.mmu = mmu;
        }

        private void ResetRegisters()
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

        //LD sp, nn (next word)
        //this.sp = mmu.rw((ushort)(pc + 1));
        //break;

        private void Dispatch(byte op)
        {
            bool throwEx = true;
            switch(op & X_MASK)
            {
                case 0:
                    switch (op & Z_MASK)
                    {
                        case 0:
                            switch (op & Y_MASK)
                            {
                                case 0: //NOP
                                    break;
                                case Y_1:
                                    //LD(nn), SP
                                    ushort nextWord = mmu.rw(pc + 1);
                                    mmu.ww(nextWord, sp);
                                    break;
                                case Y_2:
                                    //STOP
                                    throw new Exception("I have no idea how to handle a stop instruction. Sorry");
                                    break;
                                case Y_3:
                                    /* JR d
                                     Final address = d + 2 + instruction address.
                                     */
                                    byte d = mmu.rb(pc + 1);
                                    pc = (ushort)(d + pc + 2); //
                                    break;
                                case Y_4:
                                    //

                                    break;
                            }
                            throwEx = false;
                            break;
                        case 1:
                        case 2:
                    }
                    break;
                case X_1:
                    break;
                case X_2:
                    break;
                case X_3:
                    break;
                    
                    
                    
            }
            if (throwEx)
            {
                throw new NotImplementedException($"The op code {op:X2} has not been implemented yet.");
            }
        }

        private void Jr(byte op)
        {

        }
    }
}
