using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator
{
    public interface IControlUnit
    {

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
    }
    public class ControlUnit : IControlUnit
    {
        MMU mMU;
        private readonly IRegisters Registers;

        public ControlUnit(MMU mMU, IRegisters reg)
        {
            this.mMU = mMU;
            this.Registers = reg;
        }
        public void Add16(ushort rhs)
        {
            Registers.Subtract = false;

            var cachedResult = (Registers.HL & 0xffff) + (rhs & 0xffff);
            Registers.HalfCarry = (Registers.HL & 0xfff) + (rhs & 0xfff) > 0xfff;
            Registers.Carry = cachedResult > 0xffff;

            Registers.HL += (ushort)cachedResult;
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
            return ((op >> 3) & 0x38) switch
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
            if (op == 0x18 || CC(op))
            {
                Registers.PC += (ushort)offset;
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

            Registers.Zero = true;
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
            byte preservedCarry = Convert.ToByte(Registers.Carry);

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
            Registers.Carry = (value & 1) == 1;
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

        public byte SRA(byte value)
        {
            byte result = (byte)(value >> 1);

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
            int result = Registers.A - value;

            Registers.Zero = result == 0;
            Registers.Subtract = true;
            Registers.HalfCarry = (Registers.A & 0x0f) < (value & 0x0f);
            Registers.Carry = Registers.A < result;
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
    }
}
