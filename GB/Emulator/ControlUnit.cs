using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator
{
    public interface IControlUnit
    {

        public int Cycles { get; set; }
        void Call(ushort addr);
        void Push(ushort value);
        ushort POP();
        void IncReg(int regId);
        void DecReg(int regId);
        void Bit(int n, byte reg);
        void XOR(byte value);
        void Add16(ushort rhs);
        void Jr(byte op, sbyte offset);
        byte RLC(byte value);
        byte RRC(byte value);
        byte RR(byte value);
        byte RL(byte value);
        byte SLA(byte value);
        byte SRA(byte value);
        byte SWAP(byte value);
        byte SRL(byte value);
        byte RES(int n, byte value);
        byte SET(int n, byte value);
        byte RLA(byte value);
        byte RLCA(byte value);
        void RET();
        void CP(int value);
        void SUBA(int RegId);
        void SUBC(int RegId);
        void SUBCv(int value);
        void ORA(int value);
        void ADDA(int value);
        void ADDC(int value);
        void AND(int value);
        void JP(int addr);
        void JPNZ(int addr);
        void JPZ(int addr);
        void RETNZ();
        void RETZ();
        void CPL();
        void RST(byte addr);
        void RETC(byte op);
        void CALLCC(byte op, ushort addr);
        void RRA();
        void SUB(byte value);
        void DAA();
        void LDSPe8(sbyte value);
        void JPCC(byte op, int addr);
        void ADDSP(sbyte regId);
    }
    public class ControlUnit : IControlUnit
    {
        MMU mMU;
        public int Cycles { get; set; }
        private readonly IRegisters Registers;

        public ControlUnit(MMU mMU, IRegisters reg)
        {
            this.mMU = mMU;
            this.Registers = reg;
            Cycles = 0;
        }

        public void Add16(ushort rhs)
        {
            Registers.Subtract = false;

            var cachedResult = (Registers.HL & 0xffff) + (rhs & 0xffff);
            Registers.HalfCarry = (Registers.HL & 0xfff) + (rhs & 0xfff) > 0xfff;
            Registers.Carry = cachedResult > 0xffff;

            Registers.HL = (ushort)cachedResult;
        }

        public void Bit(int n, byte reg)
        {
            var mask = 1 << n;
            Registers.Zero = (mask & reg) == 0;
            Registers.Subtract = false;
            Registers.HalfCarry = true;
        }

        public void Call(ushort addr)
        {
            Push(Registers.PC);
            Registers.PC = addr;
        }

        private bool CC(byte op)
        {
            return ((op >> 3) & 0x3) switch
            {
                0 => !Registers.Zero,
                1 => Registers.Zero,
                2 => !Registers.Carry,
                3 => Registers.Carry,
                _ => throw new NotImplementedException()
            };
        }

        public void IncReg(int regId)
        {
            byte result = Registers.GetRegById(regId);
            result++;

            // Z 0 H -
            Registers.Zero = result == 0;
            Registers.Subtract = false;
            Registers.HalfCarry = (result & 0xf) == 0;

            Registers.SetRegById(regId, result);
        }

        public void DecReg(int regId)
        {
            byte result = Registers.GetRegById(regId);
            result--;

            // Z 1 H -
            Registers.Zero = result == 0;
            Registers.Subtract = true;
            Registers.HalfCarry = (result & 0xf) == 0xf;

            Registers.SetRegById(regId, result);
        }

        public void Jr(byte op, sbyte offset)
        {
            Cycles += OpTiming.JR_N;
            if (op == 0x18 || CC(op))
            {
                Registers.PC += (ushort)offset;
                Cycles += OpTiming.JR_Y;
            }
        }

        public void Push(ushort value)
        {
            Registers.SP -= 2;
            WriteWord(Registers.SP, value);
        }

        void WriteWord(ushort dest, ushort value)
        {
            mMU.WriteWord(dest, value);
        }

        ushort ReadWord(ushort addr)
        {
            return mMU.rw(addr);
        }
        public void XOR(byte value)
        {
            Registers.A ^= value;

            Registers.Zero = Registers.A == 0;
            Registers.Subtract = false;
            Registers.HalfCarry = false;
            Registers.Carry = false;
        }

        public ushort POP()
        {
            var result = ReadWord(Registers.SP);
            Registers.SP += 2;
            return result;
        }

        public byte RLC(byte value)
        {
            byte result = (byte)(value << 1 | value >> 7);
            Registers.Zero = result == 0;
            Registers.Carry = (value & 0x80) == 0x80;
            Registers.Subtract = false;
            Registers.HalfCarry = false;
            return result;
        }

        public byte RRC(byte value)
        {
            byte result = (byte)(value >> 1 | value << 7);
            Registers.Zero = result == 0;
            Registers.Carry = (value & 1) == 1;
            Registers.Subtract = false;
            Registers.HalfCarry = false;
            return result;
        }

        public byte RR(byte value)
        {
            byte preservedCarry = Registers.Carry ? 0x80 : 0;

            byte result = (byte)(value >> 1 | preservedCarry);

            Registers.Zero = result == 0;
            Registers.Carry = (value & 1) == 1;
            Registers.Subtract = false;
            Registers.HalfCarry = false;

            return result;
        }

        public byte RL(byte value)
        {
            byte preservedCarry = Convert.ToByte(Registers.Carry);

            byte result = (byte)(value << 1 | preservedCarry);

            Registers.Zero = result == 0;
            Registers.Carry = (value & 0x80) > 1;
            Registers.Subtract = false;
            Registers.HalfCarry = false;

            return result;
        }

        public byte SLA(byte value)
        {
            byte result = (byte)(value << 1);

            Registers.Zero = result == 0;
            Registers.Carry = (value & 0x80) == 0x80;
            Registers.Subtract = false;
            Registers.HalfCarry = false;

            return result;
        }

        //this needs to be tested thoroughly
        public byte SRA(byte value)
        {
            byte result = (byte)(((value & 0x80) == 0x80 ? 0x80 : 0x0)|(value >> 1));
            
            Registers.Zero = result == 0;
            Registers.Carry = (value & 0x1) == 1;
            Registers.Subtract = false;
            Registers.HalfCarry = false;

            return result;
        }

        public byte SWAP(byte value)
        {
            Registers.Zero = value == 0;
            Registers.Subtract = false;
            Registers.Carry = false;
            Registers.HalfCarry = false;

            return (byte)(value >> 4 | value << 4);
        }

        public byte SRL(byte value)
        {
            byte result = (byte)(value >> 1);

            Registers.Carry = (value & 1) == 1;
            Registers.Zero = result == 0;

            Registers.Subtract = false;
            Registers.HalfCarry = false;

            return result;
        }

        public byte RES(int n, byte value)
        {
            var mask = 1 << n;
            mask = ~mask;
            return (byte)(mask & value);

        }

        public byte SET(int n, byte value)
        {
            var mask = 1 << n;
            return (byte)(mask | value);
        }

        public byte RLA(byte value)
        {
            //preserve old carry as carry-in, and set new carry. zero everything else.
            byte preservedCarry = Convert.ToByte(Registers.Carry);

            Registers.Carry = (value & 0x80) == 0x80;
            Registers.Zero = false;
            Registers.HalfCarry = false;
            Registers.Subtract = false;
            return (byte)((value << 1) | preservedCarry);
        }

        public byte RLCA(byte value)
        {
            Registers.Zero = false;
            Registers.HalfCarry = false;
            Registers.Subtract = false;
            Registers.Carry = (value & 0x80) == 0x80;

            return (byte)((value << 1) | Convert.ToByte(Registers.Carry));
        }

        public void RET()
        {
            Registers.PC = POP();
        }

        public void CP(int value)
        {
            byte result = (byte)(Registers.A - value);

            Registers.Zero = result == 0;
            Registers.Subtract = true;
            Registers.HalfCarry = (Registers.A & 0x0f) < (value & 0x0f);
            Registers.Carry = Registers.A < value;
        }

        public void SUBA(int RegId)
        {
            Registers.Subtract = true;
            byte value = Registers.GetRegById(RegId);
            Registers.Carry = value > Registers.A;
            Registers.A -= value;
            Registers.Zero = Registers.A == 0;
            Registers.HalfCarry = (Registers.A & 0x0f) < (value & 0x0f);
        }

        public void SUBC(int RegId)
        {
            Registers.Subtract = true;
            byte value = Registers.GetRegById(RegId);

            bool carry = value > Registers.A;
            Registers.A = (byte)(Registers.A - value - Convert.ToByte(Registers.Carry));
            Registers.Zero = Registers.A == 0;

            Registers.HalfCarry = ((Registers.A & 0x0f) - Convert.ToByte(Registers.Carry)) < (value & 0x0f);
            Registers.Carry = carry;
        }

        public void ORA(int value)
        {
            Registers.A |= (byte)value;

            Registers.Zero = Registers.A == 0;
            Registers.Subtract = false;
            Registers.HalfCarry = false;
            Registers.Carry = false;
        }

        public void ADDA(int value)
        {
            Registers.Subtract = false;
            byte result = (byte)(Registers.A + value);
            Registers.Carry = value + Registers.A > 255;
            Registers.HalfCarry = (Registers.A & 0x0f) + (value & 0x0f) > 0x0f;
            Registers.Zero = result == 0;
            Registers.A = result;
        }

        public void ADDC(int value)
        {
            int preservedCarry = Convert.ToInt32(Registers.Carry);
            int result = Registers.A + value + preservedCarry;

            Registers.Zero = (byte)result == 0;
            Registers.Subtract = false;
            Registers.HalfCarry = (Registers.A & 0x0f) + (value & 0x0f) + preservedCarry > 0x0f;
            Registers.Carry = result > 255;

            Registers.A = (byte)result;

        }

        public void AND(int value)
        {
            Registers.Subtract = false;
            Registers.HalfCarry = true;
            Registers.Carry = false;

            Registers.A &= (byte)value;
            Registers.Zero = Registers.A == 0;
        }

        public void JP(int addr)
        {
            Registers.PC = (ushort)addr;
        }

        public void JPNZ(int addr)
        {
            Cycles += 12;
            if (!Registers.Zero)
            {
                JP(addr);
                Cycles += 4;
            }
        }

        public void JPZ(int addr)
        {
            Cycles += 12;
            if (Registers.Zero)
            {
                JP(addr);
                Cycles += 4;
            }
        }

        public void RETNZ()
        {
            Cycles += OpTiming.NO_RET;
            if (!Registers.Zero)
            {
                Cycles += OpTiming.RET_C;
                RET();
            }
        }

        public void RETZ()
        {
            Cycles += OpTiming.NO_RET;
            if (Registers.Zero)
            {
                Cycles += OpTiming.RET_C;
                RET();
            }
        }

        public void CPL()
        {
            Registers.Subtract = true;
            Registers.HalfCarry = true;
            Registers.A = (byte)~Registers.A;
            Cycles += OpTiming.ARITHMETIC;
        }

        public void RST(byte addr)
        {
            this.Call(addr);
            Cycles += OpTiming.RST;
        }

        public void RETC(byte op)
        {
            Cycles += OpTiming.ARITHMETIC_LOAD;
            if (CC(op))
            {
                Cycles += OpTiming.RET_C;
                RET();
            }
        }

        public void CALLCC(byte op, ushort addr)
        {
            Cycles += OpTiming.RET_C;
            if (CC(op))
            {
                Cycles += OpTiming.RET_C;
                Call(addr);
            }
        }

        public void RRA()
        {
            //preserve old carry as carry-in, and set new carry. zero everything else.
            var preservedCarry = Registers.Carry;
            byte value = Registers.A;
            Registers.Carry = (value & 0x1) == 1;
            Registers.Zero = false;
            Registers.HalfCarry = false;
            Registers.Subtract = false;
            Registers.A = (byte)((value >> 1) | (preservedCarry ? 0x80 : 0));
            Cycles += OpTiming.ARITHMETIC;
        }

        public void SUB(byte value)
        {
            Registers.Subtract = true;
            Registers.Carry = value > Registers.A;
            Registers.A -= value;
            Registers.Zero = Registers.A == 0;
            Registers.HalfCarry = (Registers.A & 0x0f) < (value & 0x0f);
            Cycles += OpTiming.ARITHMETIC_LOAD;
        }

        public void SUBCv(int value)
        {
            Registers.Subtract = true;
            bool carry = value > Registers.A;
            Registers.A = (byte)(Registers.A - value - Convert.ToByte(Registers.Carry));
            Registers.Zero = Registers.A == 0;

            Registers.HalfCarry = ((Registers.A & 0x0f) - Convert.ToByte(Registers.Carry)) < (value & 0x0f);
            Registers.Carry = carry;
            Cycles += OpTiming.ARITHMETIC_LOAD;
        }

        public void DAA()
        {
            byte adjustmen = 0;
            if(Registers.Carry || (Registers.A > 0x99 &&  !Registers.Subtract))
            {
                adjustmen = 0x60;
                Registers.Carry = true;
            }
            if (Registers.HalfCarry || ((Registers.A & 0x0f) > 0x09 && !Registers.Subtract))
            {
                adjustmen += 0x06;
            }

            Registers.A += Registers.Subtract ? (byte)-adjustmen : adjustmen;
            Registers.Zero = Registers.A == 0;
            Registers.HalfCarry = false;
            Cycles += OpTiming.ARITHMETIC;
        }

        public void LDSPe8(sbyte value)
        {
            var result = Registers.SP + value;
            Registers.HL = (ushort)result;
            Registers.Subtract = false;
            Registers.Zero = false;
            Registers.HalfCarry = (result & 0xf) < (Registers.SP & 0x0f);
            Registers.Carry = (result & 0xff) < (Registers.SP & 0xff);
            Cycles += OpTiming.LDH;
        }

        public void JPCC(byte op, int addr)
        {
            Cycles += OpTiming.LDH;
            if (CC(op))
            {
                Cycles += OpTiming.ARITHMETIC;
                JP(addr);
            }
        }

        public void ADDSP(sbyte value)
        {
            Cycles += OpTiming.ADD_SP;
            var result = Registers.SP + value;
            
            Registers.Subtract = false;
            Registers.Zero = false;
            Registers.HalfCarry = (result & 0xf) < (Registers.SP & 0x0f);
            Registers.Carry = (result & 0xff) < (Registers.SP & 0xff);

            Registers.SP = (ushort)result;
        }
    }
}
