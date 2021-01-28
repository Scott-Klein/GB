using System;

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

        private int m;
        private int t;

        private static ushort Combine(byte hi, byte lo)
        {
            return (ushort)((hi << 8) | lo);
        }

        public byte A
        {
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

        //public for unit testing
        public ushort PC
        {
            get
            { return pc; }
        }

        private Clock clock;
        private MMU mmu;
        private bool zero;
        private bool carry;
        private bool subtract;
        private bool halfCarry;

        public bool Zero { get { return zero; } }
        public bool Carry { get { return carry; } }
        public bool Sub { get { return subtract; } }
        public bool HalfCarry { get { return halfCarry; } }

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
            switch (op)
            {
                case 0:
                    //nop;
                    break;

                case 0xcb:
                    DispatchCb();
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
                    LoadWordRegisterPair1((ushort)(op >> 4), NextWord());
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
                    WriteByte(RegisterPair2(op >> 4), a);
                    break;
                case 0xa:
                case 0x1a:
                case 0x2a:
                case 0x3a:
                    a = RegisterPair2Indirect(op>>4);
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
                case 0xe0:
                    //LDH (a8), A which is LD (0xff00+a8), a
                    WriteByte(0xff00 + NextByte(), a);
                    break;
                case 0xf0:
                    a = ReadByte(0xff00 + NextByte());
                    break;
                case var o when (o & 0xa8) == 0xa8:
                    XOR(GetReg(o & 0x7));
                    break;
                case 0x0e:
                case 0x1e:
                case 0x2e:
                case 0x3e:
                    SetReg(op >> 3, NextByte());
                    break;
                case 0xe2:
                    WriteByte(0xff00 + c, a);
                    break;
                case 0xf2:
                    a = ReadByte(0xff00 + c);
                    break;
                case 0x0c:
                case 0x1c:
                case 0x2c:
                case 0x3c:
                    IncReg(op >> 3);
                    break;
                case 0x70:
                case 0x71:
                case 0x72:
                case 0x73:
                case 0x74:
                case 0x75:
                case 0x77:
                    WriteByte(HL, GetReg(0xf & op));
                    break;
                case 0xcd:
                    Call(NextWord());
                    break;
                default:
                    throw new NotImplementedException($"The op code {op:X2} has not been implemented yet.");
            }
        }

        private int RegisterPair2(int id)
        {
            return id switch
            {
                0 => BC,
                1 => DE,
                2 => HL++,
                3 => HL--,
                _ => throw new ArgumentOutOfRangeException("Id", id, $"Id for register pair 2 was out of range. Was:{id}")
            };
        }

        private void Call(ushort addr)
        {
            Push(pc);
            pc = addr;
        }

        private void Push(ushort value)
        {
            sp -= 2;
            WriteWord(sp, value);
        }

        private void IncReg(int v)
        {
            byte result = GetReg(v);
            result++;
            
            // Z 0 H -
            zero = result == 0;
            subtract = false;
            halfCarry = (result & 0xf) == 0;

            SetReg(v, result);
        }

        private byte RegisterPair2Indirect(int id)
        {
            return id switch
            {
                0 => ReadByte(BC),
                1 => ReadByte(DE),
                2 => ReadByte(HL++),
                3 => ReadByte(HL--),
                _ => throw new ArgumentOutOfRangeException("Id", id, $"Id for register pair 2 was out of range. Was:{id}")
            };
        }

        private byte ReadByte(int addr)
        {
            return mmu.rb((ushort)addr);
        }
        private void WriteByte(int addr, byte value)
        {
            mmu.wb((ushort)addr, value);
        }
        
        private void WriteWord(int addr, ushort value)
        {
            mmu.WriteWord(addr, value);
        }


        private void DispatchCb()
        {
            byte op = mmu.rb(pc++);
            switch (op)
            {
                case var o when o >= 0x40 && o <= 0x7f:
                    Bit((o >> 4), GetReg(o & 0x7));
                    break;

                default:
                    throw new NotImplementedException($"The op code {op:X2} has not been implemented yet.");
            }
        }

        private void Bit(int n, byte reg)
        {
            var mask = 1 << n;
            zero = (mask & reg) == 0;
            subtract = false;
            halfCarry = true;
        }

        private byte GetReg(int id)
        {
            switch (id)
            {
                case 0:
                    return b;

                case 1:
                    return c;

                case 2:
                    return d;

                case 3:
                    return e;

                case 4:
                    return h;

                case 5:
                    return l;

                case 6:
                    return mmu.rb(HL);

                case 7:
                    return a;

                default:
                    throw new ArgumentOutOfRangeException("Register id", id, $"Register {id} was not within range of the r8 table");
            }
        }

        private void SetReg(int id, byte val)
        {
            switch (id)
            {
                case 0:
                    b = val;
                    break;
                case 1:
                    c = val;
                    break;
                case 2:
                    d = val;
                    break;
                case 3:
                    e = val;
                    break;
                case 4:
                    h = val;
                    break;
                case 5:
                    l = val;
                    break;
                case 6:
                    WriteByte(HL, val);
                    break;
                case 7:
                    a = val;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("ID", id, $"Id was not in the range for a valid Register: {id}");
            }
        }

        private void XOR(byte value)
        {
            a ^= value;

            zero = true;
            subtract = false;
            halfCarry = false;
            carry = false;
        }

        private void ALU(byte op)
        {
            switch (op & 0x38 >> 3)
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


        //WriteByte(Reg16Rp2(v), a);


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

        private void LoadWordRegisterPair1(ushort reg, ushort addr)
        {
            switch (reg)
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

        private byte NextByte()
        {
            return mmu.rb(pc++);
        }

        private void Ld(byte op)
        {
        }

        private void Jr(byte op)
        {
            sbyte signedOffset = (sbyte)NextByte();
            if (op == 0x18 || cc(op))
            {
                
                pc += (ushort)signedOffset;
            }
        }

        public bool cc(byte op)
        {
            return ((op >> 3) & 0x38) switch
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