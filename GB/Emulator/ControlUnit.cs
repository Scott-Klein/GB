﻿using System;
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
        void Bit(int n, byte reg);
        void XOR(byte value);
        void Add16(ushort rhs);
        void Jr(byte op, sbyte offset);
        byte RLC(byte value);
        byte RRC(byte value);
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
            Registers.Carry = value == 0x80;
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
    }
}