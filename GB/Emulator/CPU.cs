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
        private bool zero;
        private bool carry;
        private bool subtract;
        private bool halfCarry;
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
            switch(op)
            {
                case 0:
                    //nop;
                    break;
                case 0x8:
                    //LD(nn),sp
                    mmu.WriteWord(NextWord(), sp);
                    break;
                case 0x10:
                    throw new Exception("STOP");
                case 0x18:
                case 0x20:
                case 0x28:
                case 0x30:
                case 0x38:
                    Jr(op);
                    break;
                case 0x01:
                case 0x11:
                case 0x21:
                case 0x31:
                    Ld16((ushort)((op & 0x30) >> 4), NextWord());
                    break;
                case 0x09:
                case 0x19:
                case 0x29:
                case 0x39:
                    AddHL((ushort)((op & 0x30) >> 4));
                        break;

                default:
                    throw new NotImplementedException($"The op code {op:X2} has not been implemented yet.");
            }
           
        }
        private void AddHL(ushort v)
        {
            subtract = false;
            halfCarry = (HL & 0xfff) + (v & 0xfff) > 0xfff;
            carry = (HL & 0xffff) + (v & 0xffff) > 0xffff;
            HL += v;
        }

        private ushort Reg16Rp(int v)
        {
            return v switch
            {
                0 => BC,
                1 => DE,
                2 => HL,
                3 => sp
            };
        }

        private void Ld16(ushort reg, ushort addr)
        {
            switch(reg)
            {
                case 0:
                    BC = NextWord();
                    break;
                case 1:
                    DE = NextWord();
                    break;
                case 2:
                    HL = NextWord();
                    break;
                case 3:
                    sp = NextWord();
                    break;
            }
        }

        private ushort NextWord()
        {
            return mmu.rw(pc++);
        }
        private void Ld(byte op)
        {

        }

        private void Jr(byte op)
        {
            if (op == 0x18 || cc(op))
            {
                byte d = mmu.rb(pc + 1);
                pc += (ushort)(d + 2); 
            }
        }
        public bool cc(byte op)
        {
            return ((op & 0x38) >> 3) switch
            {
                0 => !zero,
                1 => zero,
                2 => !carry,
                3 => carry,
                _ => throw new NotImplementedException()
            };
        }
    }
}
