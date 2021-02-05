using System;

namespace GB.Emulator
{
    public sealed class CPU
    {
        private Clock clock;
        private IControlUnit ControlUnit;
        private bool IME;
        private bool Halt1;
        private int Cycles;
        private MMU mmu;
        private int pendingIME;
        private IRegisters Registers;
        public CPU(MMU mmu, Clock clock)
        {
            this.clock = clock;
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

        /// <summary>
        /// To test the cpu, we have this tick overload that allows injecting instructions
        /// from the testing framework.
        /// </summary>
        /// <param name="op">The 8 bit op code to inject and execute</param>
        public void Tick(byte op)
        {
            Dispatch(op);
            Registers.PC &= 0xffff; //mask the pc.
        }

        public void Tick()
        {
            byte op;
            Cycles = 0;
            InterruptRoutine();

            if (!Halt1)
            {
                //fetch;
                op = NextByte();
            }
            else
            {
                op = 0x76;//still in halt.
            }

            Dispatch(op);

            HandleTiming();
        }

        public void HandleTiming()
        {
            clock.Tick(Cycles + ControlUnit.Cycles);
            ControlUnit.Cycles = 0;
            mmu.Tick();
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
                    Cycles += OpTiming.LD_SP;
                    break;

                //RLCA
                case 0x07:
                    Registers.A = ControlUnit.RLA(Registers.A);
                    Cycles += OpTiming.SHIFT;
                    break;

                //RLA
                case 0x17:
                    Registers.A = ControlUnit.RLA(Registers.A);
                    Cycles += OpTiming.SHIFT;
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
                    Cycles += OpTiming.LD_WORD;
                    break;

                case 0x09:
                case 0x19:
                case 0x29:
                case 0x39:
                    ControlUnit.Add16(Registers.ReadWordRegisterPair1(op >> 4));
                    Cycles += OpTiming.ARITHMETIC_LOAD;
                    break;

                //LD (BC), A
                case 0x02:
                case 0x12:
                case 0x22:
                case 0x32:
                    WriteByte(Registers.ReadWordRegisterPair3(op >> 4), Registers.A);
                    Cycles += OpTiming.STORE_BYTE;
                    break;

                case 0xa:
                case 0x1a:
                case 0x2a:
                case 0x3a:
                    Registers.A = Registers.RegisterPair2Indirect(op >> 4);
                    Cycles += OpTiming.STORE_BYTE;
                    break;

                case 0x03:
                    Registers.BC++;
                    Cycles += OpTiming.INC_WORD_REG;
                    break;

                case 0x13:
                    Registers.DE++;
                    Cycles += OpTiming.INC_WORD_REG;
                    break;

                case 0x23:
                    Registers.HL++;
                    Cycles += OpTiming.INC_WORD_REG;
                    break;

                case 0x33:
                    Registers.SP++;
                    Cycles += OpTiming.INC_WORD_REG;
                    break;

                case 0x0b:
                    Registers.BC--;
                    Cycles += OpTiming.INC_WORD_REG;
                    break;

                case 0x1b:
                    Registers.DE--;
                    Cycles += OpTiming.INC_WORD_REG;
                    break;

                case 0x2b:
                    Registers.HL--;
                    Cycles += OpTiming.INC_WORD_REG;
                    break;

                case 0x3b:
                    Registers.SP--;
                    Cycles += OpTiming.INC_WORD_REG;
                    break;

                case 0xe0:
                    //LDH (a8), A which is LD (0xff00+a8), a
                    WriteByte(0xff00 + NextByte(), Registers.A);
                    Cycles += OpTiming.LDH;
                    break;

                case 0xf0:
                    Registers.A = ReadByte(0xff00 + NextByte());
                    Cycles += OpTiming.LDH;
                    break;
                //case var o when (o & 0xa8) == 0xa8:
                //    ControlUnit.XOR(Registers.GetRegById(o & 0x7));
                //    break;
                case 0x0e:
                case 0x1e:
                case 0x2e:
                case 0x3e:
                    Registers.SetRegById(op >> 3, NextByte());
                    Cycles += OpTiming.STORE_BYTE;
                    break;

                case 0xe2:
                    WriteByte(0xff00 + Registers.C, Registers.A);
                    Cycles += OpTiming.STORE_BYTE;
                    break;

                case 0xf2:
                    Registers.A = ReadByte(0xff00 + Registers.C);
                    Cycles += OpTiming.STORE_BYTE;
                    break;

                case 0x0c:
                case 0x1c:
                case 0x2c:
                case 0x3c:
                    ControlUnit.IncReg(op >> 3);
                    Cycles += OpTiming.ARITHMETIC;
                    break;

                case 0x0d:
                case 0x2d:
                case 0x3d:
                case 0x1d:
                    ControlUnit.DecReg(op >> 3);
                    Cycles += OpTiming.ARITHMETIC;
                    break;
                case 0x70:
                case 0x71:
                case 0x72:
                case 0x73:
                case 0x74:
                case 0x75:
                case 0x77:
                    WriteByte(Registers.HL, Registers.GetRegById(0xf & op));
                    Cycles += OpTiming.STORE_BYTE;
                    break;

                case 0xcd:
                    ControlUnit.Call(NextWord());
                    Cycles += OpTiming.CALL;
                    break;

                case 0xc9:
                    ControlUnit.RET();
                    Cycles += OpTiming.RET;
                    break;

                case 0x48:
                case 0x49:
                case 0x4a:
                case 0x4b:
                case 0x4c:
                case 0x4d:
                case 0x4f:
                    Registers.C = Registers.GetRegById(0x7 & op);
                    Cycles += OpTiming.LD;
                    break;

                case 0x4e:
                    Registers.C = Registers.GetRegById(0x7 & op);
                    Cycles += OpTiming.STORE_BYTE;
                    break;

                case var o when (o >= 0x58 && o <= 0x5d) || o == 0x5f:
                    Registers.E = Registers.GetRegById(0x7 & op);
                    Cycles += OpTiming.LD;
                    break;

                case 0x5e:
                    Registers.E = Registers.GetRegById(0x7 & op);
                    Cycles += OpTiming.STORE_BYTE;
                    break;

                case var o when (o >= 0x68 && o <= 0x6d) || o == 0x6f:
                    Registers.L = Registers.GetRegById(0x7 & op);
                    Cycles += OpTiming.LD;
                    break;

                case 0x6e:
                    Registers.L = Registers.GetRegById(0x7 & op);
                    Cycles += OpTiming.STORE_BYTE;
                    break;

                case var o when (o >= 0x78 && o <= 0x7d) || o == 0x7f:
                    Registers.A = Registers.GetRegById(0x7 & op);
                    Cycles += OpTiming.LD;
                    break;

                case 0x7e:
                    Registers.A = Registers.GetRegById(0x7 & op);
                    Cycles += OpTiming.STORE_BYTE;
                    break;

                case 0x06:
                case 0x16:
                case 0x26:
                    Registers.SetRegById((op >> 4) << 1, NextByte());
                    Cycles += OpTiming.STORE_BYTE;
                    break;
                case 0x36:
                    Registers.SetRegById((op >> 4) << 1, NextByte());
                    Cycles += OpTiming.LD_WORD;
                    break;

                case 0xc1:
                case 0xd1:
                case 0xe1:
                case 0xf1:
                    Registers.LoadWordRegisterPair2((op >> 4) & 3, ControlUnit.POP());
                    Cycles += OpTiming.POP;
                    break;

                case 0xc5:
                case 0xd5:
                case 0xe5:
                case 0xf5:
                    ControlUnit.Push(Registers.ReadWordRegisterPair2((op >> 4) & 3));
                    Cycles += OpTiming.PUSH;
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
                    ControlUnit.IncReg((op >> 4) << 1);
                    Cycles += OpTiming.ARITHMETIC;
                    break;
                case 0x34:
                    ControlUnit.IncReg((op >> 4) << 1);
                    Cycles += OpTiming.ARITHMETIC_LOAD;
                    break;

                case 0xfe:
                    ControlUnit.CP(NextByte());
                    Cycles += OpTiming.ARITHMETIC_LOAD;
                    break;

                case var o when o >= 0xb8 && o <= 0xbf && o != 0xbe:
                    ControlUnit.CP(Registers.GetRegById(o & 0x7));
                    Cycles += OpTiming.ARITHMETIC;
                    break;

                case 0xbe:
                    ControlUnit.CP(Registers.GetRegById(op & 0x7));
                    Cycles += OpTiming.ARITHMETIC_LOAD;
                    break;

                case var o when o >= 0xa8 && o <= 0xaf && o != 0xae:
                    ControlUnit.XOR(Registers.GetRegById(o & 0x7));
                    Cycles += OpTiming.ARITHMETIC;
                    break;

                case 0xae:
                    ControlUnit.XOR(Registers.GetRegById(op & 0x7));
                    Cycles += OpTiming.ARITHMETIC_LOAD;
                    break;

                case 0xea:
                    WriteByte(NextWord(), Registers.A);
                    Cycles += OpTiming.LDH;
                    break;

                case 0xfa:
                    Registers.A = mmu.rb(NextWord());
                    Cycles += OpTiming.LDH;
                    Cycles += OpTiming.ARITHMETIC;
                    break;

                case 0x76:
                    Cycles += OpTiming.ARITHMETIC;
                    Halt1 = true;
                    if (!IME && (mmu.IE & mmu.IF) > 0)
                    {
                        Halt1 = false;
                    }
                    break;

                case 0xfb:
                    //Set Interrupt master enable.
                    pendingIME = 2;
                    Cycles += OpTiming.EI;
                    break;

                case 0xf3:
                    //Disable interrupt;
                    IME = false;
                    Cycles += OpTiming.EI;
                    break;

                case 0xf6:
                    ControlUnit.ORA(NextByte());
                    Cycles += OpTiming.ARITHMETIC_LOAD;
                    break;

                case 0xd9:
                    //RETI
                    pendingIME = 1;
                    ControlUnit.RET();
                    Cycles += OpTiming.RET;
                    break;

                case var o when o >= 0xb0 && o <= 0xb7 && o != 0xb6:
                    ControlUnit.ORA(Registers.GetRegById(o & 0x7));
                    Cycles += OpTiming.ARITHMETIC;
                    break;
                case 0xb6:
                    ControlUnit.ORA(Registers.GetRegById(op & 0x7));
                    Cycles += OpTiming.ARITHMETIC_LOAD;
                    break;

                //suba
                case var o when o >= 0x90 && o <= 0x97 && o != 0x96:
                    ControlUnit.SUBA(0x7 & o);
                    Cycles += OpTiming.ARITHMETIC;
                    break;

                case 0x96:
                    ControlUnit.SUBA(0x7 & op);
                    Cycles += OpTiming.ARITHMETIC_LOAD;
                    break;

                case var o when o >= 0x98 && o <= 0x9f && o != 0x9e:
                    ControlUnit.SUBC(0x7 & o);
                    Cycles += OpTiming.ARITHMETIC;
                    break;

                case 0x9e:
                    ControlUnit.SUBC(0x7 & op);
                    Cycles += OpTiming.ARITHMETIC_LOAD;
                    break;

                case var o when o >= 0x40 && o <= 0x7f && (o & 0x7) != 0x6 && (o & 0xf0) != 0x70:
                    //Catch all cases where load takes 4 cycles.
                    LoadByte(o);
                    Cycles += OpTiming.LD;
                    break;
                case var o when o >= 0x40 && o <= 0x7f:
                    //catch the rest of the cases which it takes 8.
                    LoadByte(o);
                    Cycles += OpTiming.STORE_BYTE;
                    break;

                case var o when o >= 0x80 && o <= 0x87 && o != 0x86:
                    ControlUnit.ADDA(Registers.GetRegById(o & 0x7));
                    Cycles += OpTiming.ARITHMETIC;
                    break;

                case 0x86:
                    ControlUnit.ADDA(Registers.GetRegById(op & 0x7));
                    Cycles += OpTiming.ARITHMETIC_LOAD;
                    break;
                case 0xc6:
                    ControlUnit.ADDA(NextByte());
                    Cycles += OpTiming.ARITHMETIC_LOAD;
                    break;
                case 0xc3:
                    ControlUnit.JP(NextWord());
                    Cycles += OpTiming.JP;
                    break;



                case var o when (o >= 0xa0 && o <= 0xa5) || o == 0xa7:
                    ControlUnit.AND(Registers.GetRegById(o & 7));
                    Cycles += OpTiming.ARITHMETIC;
                    break;
                case 0xa6:
                    Cycles += OpTiming.ARITHMETIC_LOAD;
                    ControlUnit.AND(Registers.GetRegById(op & 7));
                    break;
                case 0x2f:
                    ControlUnit.CPL();
                    break;
                case 0xe6:
                    ControlUnit.AND(NextByte());
                    Cycles += OpTiming.ARITHMETIC_LOAD;
                    break;
                case 0xcf:
                    ControlUnit.RST(0x8);
                    break;
                case 0xdf:
                    ControlUnit.RST(0x18);
                    break;
                case 0xef:
                    ControlUnit.RST(0x28);
                    break;
                case 0xff:
                    ControlUnit.RST(0x38);
                    break;
                case 0xc7:
                    ControlUnit.RST(0);
                    break;
                case 0xd7:
                    ControlUnit.RST(0x10);
                    break;
                case 0xe7:
                    ControlUnit.RST(0x20);
                    break;
                case 0xf7:
                    ControlUnit.RST(0x30);
                    break;
                case 0xe9:
                    ControlUnit.JP(Registers.HL);
                    Cycles += OpTiming.ARITHMETIC;
                    break;
                case 0x37:
                    Registers.Carry = true;
                    Registers.Negative = false;
                    Registers.HalfCarry = false;
                    Cycles += OpTiming.ARITHMETIC;
                    break;
                case 0x3f:
                    Registers.Carry = !Registers.Carry;
                    Registers.Negative = false;
                    Registers.HalfCarry = false;
                    Cycles += OpTiming.ARITHMETIC;
                    break;
                case 0xd8:
                case 0xc8:
                case 0xc0:
                case 0xd0:
                    ControlUnit.RETC(op);
                    break;
                case 0xc4:
                case 0xd4:
                case 0xcc:
                case 0xdc:
                    ControlUnit.CALLCC(op, NextWord());
                    break;
                case 0x1f:
                    ControlUnit.RRA();
                    break;
                case 0xee:
                    ControlUnit.XOR(NextByte());
                    Cycles += OpTiming.ARITHMETIC_LOAD;
                    break;
                case 0xd6:
                    ControlUnit.SUB(NextByte());
                    break;
                case 0xce:
                    ControlUnit.ADDC(NextByte());
                    Cycles += OpTiming.ARITHMETIC_LOAD;
                    break;
                case 0xde:
                    ControlUnit.SUBCv(NextByte());
                    break;
                case var o when (o >= 0x88 && o <= 0x8d) || o == 0x8f:
                    ControlUnit.ADDC(Registers.GetRegById(o & 0x7));
                    Cycles += OpTiming.ARITHMETIC;
                    break;
                case 0x8e:
                    ControlUnit.ADDC(Registers.GetRegById(op & 0x7));
                    Cycles += OpTiming.ARITHMETIC_LOAD;
                    break;
                case 0x27:
                    ControlUnit.DAA();
                    break;
                case 0xf8:
                    ControlUnit.LDSPe8((sbyte)NextByte());
                    break;
                case 0xca:
                case 0xda:
                case 0xd2:
                case 0xc2:
                    ControlUnit.JPCC(op, NextWord());
                    break;
                case 0xf9:
                    Registers.SP = Registers.HL;
                    Cycles = OpTiming.ARITHMETIC_LOAD;
                    break;
                case 0xe8:
                    ControlUnit.ADDSP((sbyte)NextByte());
                    break;
                default:
                    throw new NotImplementedException($"The op code {op:X2} has not been implemented yet.");
            }
        }

        private void LoadByte(byte op)
        {
            var regData = Registers.GetRegById(0x7 & op);
            Registers.SetRegById((op >> 3) & 0x7, regData);
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
            clock.Tick(12);
        }

        private void InterruptRoutine()
        {
            mmu.IF |= (byte)clock.IF;
            if (!IME && pendingIME > 0)
            {
                IME = --pendingIME == 0;
            }

            //Jump to vector
            if (IME && (mmu.IE & mmu.IF) != 0)
            {
                Halt1 = false;
                IME = false;
                switch (mmu.IF)
                {
                    case var f when (f & mmu.IE & 0x1) == 0x1:
                        mmu.IF = (byte)(mmu.IF & 0xfe);
                        //vblank
                        ControlUnit.Call(0x40);
                        break;

                    case var f when (f & mmu.IE & 0x2) == 0x2:
                        //LCD STAT
                        mmu.IF = (byte)(mmu.IF & 0xfd);
                        ControlUnit.Call(0x48);
                        break;

                    case var f when (f & mmu.IE & 0x4) == 0x4:
                        mmu.IF = (byte)(mmu.IF & 0xfb);
                        ControlUnit.Call(0x50);
                        //Timer Interrupt
                        break;

                    case var f when (f & mmu.IE & 0x8) == 0x8:
                        mmu.IF = (byte)(mmu.IF & 0xf7);
                        ControlUnit.Call(0x58);
                        //serial
                        break;

                    case var f when (f & mmu.IE & 0x10) == 0x10:
                        mmu.IF = (byte)(mmu.IF & 0xef);
                        ControlUnit.Call(0x60);
                        //joypad.
                        break;
                }
            }
        }
        private byte NextByte()
        {
            return mmu.rb(Registers.PC++);
        }

        private ushort NextWord()
        {
            var word = mmu.rw(Registers.PC++);
            Registers.PC++;
            return word;
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
    }
}