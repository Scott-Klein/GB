using System;

namespace GB.Emulator
{
    public sealed class CPU
    {
        IRegisters Registers;
        IControlUnit ControlUnit;
        private int m;
        private int t;
        private bool IME;
        private int pendingIME;
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
            //
            InterruptRoutine();
            //fetch;
            var op = NextByte();
            Dispatch(op);
            Registers.PC &= 0xffff; //mask the pc.

            mmu.Tick();
        }
        private void InterruptRoutine()
        {
            if (!IME && pendingIME-- == 0)
            {
                IME = true;
            }

            //Jump to vector
            if (IME && (mmu.IE & mmu.IF) != 0)
            {

                ControlUnit.Push(Registers.PC);
                switch (mmu.IF)
                {
                    case var f when (f & mmu.IE & 0x1) == 0x1:
                        //vblank
                        Registers.PC = 0x40;
                        break;
                    case var f when (f & mmu.IE & 0x2) == 0x2:
                        //LCD STAT
                        Registers.PC = 0x48;
                        break;
                    case var f when (f & mmu.IE & 0x4) == 0x4:
                        Registers.PC = 0x50;
                        //Timer Interrupt
                        break;
                    case var f when (f & mmu.IE & 0x8) == 0x8:
                        Registers.PC = 0x58;
                        //serial
                        break;
                    case var f when (f & mmu.IE & 0x10) == 0x10:
                        Registers.PC = 0x60;
                        //joypad.
                        break;
                }
            }


        }
        private void Dispatch(byte op)
        {
            switch (op)
            {
                //nop;
                case 0:
                    break;

                case 0xcb:
                    DispatchCb();
                    break;

                //LD(nn),sp
                case 0x8:

                    mmu.WriteWord(NextWord(), Registers.SP);
                    break;

                //RLCA
                case 0x07:
                    Registers.A = ControlUnit.RLA(Registers.A);
                    break;

                //RLA
                case 0x17:
                    Registers.A = ControlUnit.RLA(Registers.A);
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
                //case var o when (o & 0xa8) == 0xa8:
                //    ControlUnit.XOR(Registers.GetRegById(o & 0x7));
                //    break;
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
                case 0x0d:
                case 0x1d:
                case 0x2d:
                case 0x3d:
                    ControlUnit.DecReg(op >> 3);
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
                case 0xc9:
                    ControlUnit.RET();
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
                    Registers.SetRegById((op>>4)<<1, NextByte());
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
                case 0x05:
                case 0x15:
                case 0x25:
                case 0x35:
                    ControlUnit.DecReg((op >> 4) << 1);
                    break;
                case 0x04:
                case 0x14:
                case 0x24:
                case 0x34:
                    ControlUnit.IncReg((op >> 4) << 1);
                    break;
                case 0xfe:
                    ControlUnit.CP(NextByte());
                    break;
                case var o when o >= 0xb8 && o <= 0xbf:
                    ControlUnit.CP(Registers.GetRegById(o & 0x7));
                    break;
                case var o when o >= 0xa8 && o <= 0xaf:
                    ControlUnit.XOR(Registers.GetRegById(o & 0x7));
                    break;
                case 0xea:
                    WriteByte(NextWord(), Registers.A);
                    break;
                case 0xfa:
                    Registers.A = NextByte();
                    break;
                case 0x76:
                    throw new Exception("HALT WHO GOES THERE");
                    break;
                case 0xfb:
                    //Set Interrupt master enable.
                    pendingIME = 1;
                    break;
                case 0xf3:
                    //Disable interrupt;
                    IME = false;
                    break;
                case 0xf6:
                    ControlUnit.ORA(NextByte());
                    break;
                case 0xd9:
                    //RETI
                    pendingIME = 1;
                    ControlUnit.RET();
                    break;
                case var o when o >= 0xb0 && o <= 0xb7:
                    ControlUnit.ORA(Registers.GetRegById(o & 0x7));
                    break;
                //suba
                case var o when o >= 0x90 && o <= 0x97:
                    ControlUnit.SUBA(0x7 & o);
                    break;
                case var o when 0 >= 0x98 && o <= 0x9f:
                    ControlUnit.SUBC(0x7 & o);
                    break;
                case var o when o >= 0x40 && o <= 0x7f:
                    var regData = Registers.GetRegById(0x7 & o);
                    Registers.SetRegById((o >> 3) & 0x7, regData);
                    break;
                case var o when o >= 0x80 && o <= 0x87:
                    ControlUnit.ADDA(Registers.GetRegById(o & 0x7));
                    break;
                case 0xc3:
                    ControlUnit.JP(NextWord());
                    break;
                case 0xc2:
                    ControlUnit.JPNZ(NextWord());
                    break;
                case 0xd2:
                    ControlUnit.JPZ(NextWord());
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
            var regId = op & 0x7;
            byte r8Value = Registers.GetRegById(regId);
            switch (op)
            {
                //RLC
                case <= 0x7:
                    Registers.SetRegById(regId, ControlUnit.RLC(r8Value));
                    break;
                //RRC
                case <= 0xf:
                    Registers.SetRegById(regId, ControlUnit.RRC(r8Value));
                    break;
                //RL
                case <= 0x17:
                    Registers.SetRegById(regId, ControlUnit.RL(r8Value));
                    break;
                //RR
                case <= 0x1f:
                    Registers.SetRegById(regId, ControlUnit.RR(r8Value));
                    break;
                //SLA
                case <= 0x27:
                    Registers.SetRegById(regId, ControlUnit.SLA(r8Value));
                    break;
                //SRA
                case <= 0x2f:
                    Registers.SetRegById(regId, ControlUnit.SRA(r8Value));
                    break;
                //SWAP
                case <= 0x37:
                    Registers.SetRegById(regId, ControlUnit.SWAP(r8Value));
                    break;
                //SRL
                case <= 0x3f:
                    Registers.SetRegById(regId, ControlUnit.SRL(r8Value));
                    break;
                //BIT
                case <= 0x7f:
                    ControlUnit.Bit(((op >> 3) & 0x7), r8Value);
                    break;
                //RES
                case <= 0xbf:
                    ControlUnit.RES(((op >> 3) & 0x7), r8Value);
                    break;
                //SET
                case <= 0xff:
                    ControlUnit.SET(((op >> 3) & 0x7), r8Value);
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