using System;

namespace GB.Emulator
{
    public sealed class CPU
    {
        IRegisters Registers;
        IControlUnit ControlUnit;
        private int m;
        private int t;

        private Clock clock;
        private MMU mmu;
        public CPU(MMU mmu)
        {
            clock = new Clock();
            this.mmu = mmu;
            this.Registers = new Registers(mmu);
            this.ControlUnit = new ControlUnit(mmu, this.Registers);
        }

        /// <summary>
        /// Constructor for injecting an observed set of registers for unit testing.
        /// </summary>
        /// <param name="mmu">
        /// Memory subsystem of the Game Boy
        /// </param>
        /// <param name="reg">
        /// A Set of registers that the CPU needs. Here the registers are being injected,
        /// so that they can be observed by unit tests.
        /// </param>
        public CPU(MMU mmu, IRegisters reg)
        {
            Registers = reg;
            clock = new Clock();
            this.mmu = mmu;
            ControlUnit = new ControlUnit(mmu, reg);
        }

        //For testing.
        public void Tick(byte op)
        {
            Dispatch(op);
            Registers.PC &= 0xffff; //mask the pc.
            clock.M++;
            clock.T += t;
        }

        public void Tick()
        {
            //fetch;
            var op = NextByte();
            Dispatch(op);
            Registers.PC &= 0xffff; //mask the pc.
            clock.M++;
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
                    mmu.WriteWord(NextWord(), Registers.SP);
                    break;

                case 0x10:
                    throw new Exception("STOP");
                case 0x18:
                case 0x20:
                case 0x28:
                case 0x30:
                case 0x38:
                    ControlUnit.Jr(op, (sbyte)NextByte());
                    break;

                case 0x01:
                case 0x11:
                case 0x21:
                case 0x31:
                    Registers.LoadWordRegisterPair1((op >> 4), NextWord());
                    break;

                case 0x09:
                case 0x19:
                case 0x29:
                case 0x39:
                    ControlUnit.Add16(Registers.ReadWordRegisterPair1(op >> 4));
                    break;

                case 0x02:
                case 0x12:
                case 0x22:
                case 0x32:
                    WriteByte(Registers.ReadWordRegisterPair3(op >> 4), Registers.A);
                    break;
                case 0xa:
                case 0x1a:
                case 0x2a:
                case 0x3a:
                    Registers.A = Registers.RegisterPair2Indirect(op>>4);
                    break;

                case 0x03:
                    Registers.BC++;
                    break;

                case 0x13:
                    Registers.DE++;
                    break;

                case 0x23:
                    Registers.HL++;
                    break;

                case 0x33:
                    Registers.SP++;
                    break;

                case 0x0b:
                    Registers.BC--;
                    break;

                case 0x1b:
                    Registers.DE--;
                    break;

                case 0x2b:
                    Registers.HL--;
                    break;

                case 0x3b:
                    Registers.SP--;
                    break;
                case 0xe0:
                    //LDH (a8), A which is LD (0xff00+a8), a
                    WriteByte(0xff00 + NextByte(), Registers.A);
                    break;
                case 0xf0:
                    Registers.A = ReadByte(0xff00 + NextByte());
                    break;
                case var o when (o & 0xa8) == 0xa8:
                    ControlUnit.XOR(Registers.GetRegById(o & 0x7));
                    break;
                case 0x0e:
                case 0x1e:
                case 0x2e:
                case 0x3e:
                    Registers.SetRegById(op >> 3, NextByte());
                    break;
                case 0xe2:
                    WriteByte(0xff00 + Registers.C, Registers.A);
                    break;
                case 0xf2:
                    Registers.A = ReadByte(0xff00 + Registers.C);
                    break;
                case 0x0c:
                case 0x1c:
                case 0x2c:
                case 0x3c:
                    ControlUnit.IncReg(op >> 3);
                    break;
                case 0x70:
                case 0x71:
                case 0x72:
                case 0x73:
                case 0x74:
                case 0x75:
                case 0x77:
                    WriteByte(Registers.HL, Registers.GetRegById(0xf & op));
                    break;
                case 0xcd:
                    ControlUnit.Call(NextWord());
                    break;
                case 0x48:
                case 0x49:
                case 0x4a:
                case 0x4b:
                case 0x4c:
                case 0x4d:
                case 0x4e:
                case 0x4f:
                    Registers.C = Registers.GetRegById(0x7 & op);
                    break;
                case 0x58:
                case 0x59:
                case 0x5a:
                case 0x5b:
                case 0x5c:
                case 0x5d:
                case 0x5e:
                case 0x5f:
                    Registers.E = Registers.GetRegById(0x7 & op);
                    break;
                case 0x68:
                case 0x69:
                case 0x6a:
                case 0x6b:
                case 0x6c:
                case 0x6d:
                case 0x6e:
                case 0x6f:
                    Registers.L = Registers.GetRegById(0x7 & op);
                    break;
                case 0x78:
                case 0x79:
                case 0x7a:
                case 0x7b:
                case 0x7c:
                case 0x7d:
                case 0x7e:
                case 0x7f:
                    Registers.A = Registers.GetRegById(0x7 & op);
                    break;
                case 0x06:
                case 0x16:
                case 0x26:
                case 0x36:
                    Registers.SetRegById((0xf & op) << 1, NextByte());
                    break;
                case 0xc1:
                case 0xd1:
                case 0xe1:
                case 0xf1:
                    Registers.LoadWordRegisterPair2((op >> 4) & 3, ControlUnit.POP());
                    break;
                case 0xc5:
                case 0xd5:
                case 0xe5:
                case 0xf5:
                    ControlUnit.Push(Registers.ReadWordRegisterPair2((op >> 4) & 3));
                    break;
                default:
                    throw new NotImplementedException($"The op code {op:X2} has not been implemented yet.");
            }
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
            byte op = mmu.rb(Registers.PC++);
            switch (op)
            {
                case var o when o >= 0x40 && o <= 0x7f:
                    ControlUnit.Bit((o >> 4), Registers.GetRegById(o & 0x7));
                    break;
                default:
                    throw new NotImplementedException($"The op code {op:X2} has not been implemented yet.");
            }
        }

        private ushort NextWord()
        {
            var word = mmu.rw(Registers.PC++);
            Registers.PC++;
            return word;
        }

        private byte NextByte()
        {
            return mmu.rb(Registers.PC++);
        }
    }
}