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
                case 0x02:
                case 0x12:
                case 0x22:
                case 0x32:
                    LDr((op >> 4) & 0x03, a);
                    break;
                case 0xa:
                case 0x1a:
                case 0x2a:
                case 0x3a:
                    LDr(a, (op >> 4) & 0x03);
                    break;
                case 0x03:
                    BC++;
                    break;
                case 0x13:
                    DE++;
                    break;
                case 0x23:
                    HL++;
                    break;
                case 0x33:
                    sp++;
                    break;
                case 0x0b:
                    BC--;
                    break;
                case 0x1b:
                    DE--;
                    break;
                case 0x2b:
                    HL--;
                    break;
                case 0x3b:
                    sp--;
                    break;
                case var o when (o & 0xc0) >> 6 == 0xa:
                    //ALU
                    ALU(op);
                    break;
                default:
                    throw new NotImplementedException($"The op code {op:X2} has not been implemented yet.");
            }
           
        }
        private void ALU(byte op)
        {
            switch(op&0x38 >> 3)
            {
                //ADD
                case 0:
                    break;
                //ADC
                case 1:
                    break;
                //SUB
                case 2:
                    break;
                //SBC
                case 3:
                    break;
                //AND
                case 4:
                    break;
                //XOR
                case 5:
                    break;
                //OR
                case 6:
                    break;
                //cp
                case 7:
                    break;
            }
        }

        private void LDr(int v, byte a)
        {
            mmu.wb(Reg16Rp2(v), a);
        }
        private void LDr(byte a, int v)
        {
            var b = mmu.rb((ushort)v);
            a = b;
        }

        private void AddHL(ushort v)
        {
            subtract = false;

            var cachedResult = (HL & 0xffff) + (v & 0xffff);
            halfCarry = (HL & 0xfff) + (v & 0xfff) > 0xfff;
            carry = cachedResult > 0xffff;

            HL += (ushort)cachedResult;
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
        private ushort Reg16Rp2(int v)
        {
            return v switch
            {
                0 => BC,
                1 => DE,
                2 => HL,
                3 => AF
            };
        }
        private void Ld16(ushort reg, ushort addr)
        {
            switch(reg)
            {
                case 0:
                    BC = addr;
                    break;
                case 1:
                    DE = addr;
                    break;
                case 2:
                    HL = addr;
                    break;
                case 3:
                    sp = addr;
                    break;
            }
        }

        private ushort NextWord()
        {
            var word = mmu.rw(pc++);
            pc++;
            return word; 
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
