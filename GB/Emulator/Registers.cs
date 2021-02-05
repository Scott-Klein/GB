using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator
{
    public interface IRegisters
    {

        ushort AF { get; set; }
        ushort BC { get; set; }
        ushort DE { get; set; }
        ushort HL { get; set; }
        bool Zero { get; set; }
        bool Carry { get; set; }
        bool Negative { get; set; }
        bool HalfCarry { get; set; }

        byte A { get; set; }
        byte B { get; set; }
        byte C { get; set; }
        byte D { get; set; }
        byte E { get; set; }
        byte H { get; set; }
        byte L { get; set; }
        byte F { get; set; }
        ushort SP { get; set; }
        ushort PC { get; set; }

        byte GetRegById(int id);
        void SetRegById(int id, byte value);
        void LoadWordRegisterPair1(int regId, ushort word);
        void LoadWordRegisterPair2(int regId, ushort word);
        ushort ReadWordRegisterPair1(int regId);
        ushort ReadWordRegisterPair2(int regId);
        ushort ReadWordRegisterPair3(int regId);
        byte RegisterPair2Indirect(int id);
        void SetAllRegs(byte value);

    }

    public class Registers : IRegisters
    {
        private readonly MMU mmu;

        public Registers(MMU mmu, bool SkipBoot = false)
        {
            if (SkipBoot)
            {
                //Set default registers
                AF = 0x1b0;
                BC = 0x13;
                DE = 0xD8;
                HL = 0x14d;
                SP = 0xfffe;
                PC = 0x100;
            }

            this.mmu = mmu;
        }

        public ushort AF
        {
            get
            {
                return Combine(A, F);
            }
            set
            {
                this.A = (byte)(value >> 8);
                this.F = (byte)(value & 0xff);
            }
        }

        public ushort BC
        {
            get
            {
                return Combine(B, C);
            }
            set
            {
                this.B = (byte)(value >> 8);
                this.C = (byte)(value & 0xff);
            }
        }

        public ushort DE
        {
            get
            {
                return Combine(D, E);
            }
            set
            {
                this.D = (byte)(value >> 8);
                this.E = (byte)(value & 0xff);
            }
        }

        public ushort HL
        {
            get
            {
                return Combine(H, L);
            }
            set
            {
                this.H = (byte)(value >> 8);
                this.L = (byte)(value & 0xff);
            }
        }

        public bool Zero { get; set; }
        public bool Carry { get; set; }
        public bool Negative { get; set; }
        public bool HalfCarry { get; set; }

        public byte A { get; set; }
        public byte B { get; set; }
        public byte C { get; set; }
        public byte D { get; set; }
        public byte E { get; set; }
        public byte H { get; set; }
        public byte L { get; set; }

        public ushort SP { get; set; }
        public ushort PC { get; set; }

        public byte F
        {
            get
            {
                return (byte)(Convert.ToInt16(this.Zero) << 7 | Convert.ToInt16(this.Negative) << 6 | Convert.ToInt16(this.HalfCarry) << 5 | Convert.ToInt16(this.Carry) << 4);
            }
            set
            {
                this.Zero = (value & 0x80) > 0;
                this.Negative = (value & 0x40) > 0;
                this.HalfCarry = (value & 0x20) > 0;
                this.Carry = (value & 0x10) > 0;
            }
        }

        private static ushort Combine(byte hi, byte lo)
        {
            return (ushort)((hi << 8) | lo);
        }

        public byte GetRegById(int id)
        {
            {
                switch (id)
                {
                    case 0:
                        return B;

                    case 1:
                        return C;

                    case 2:
                        return D;

                    case 3:
                        return E;

                    case 4:
                        return H;

                    case 5:
                        return L;

                    case 6:
                        return mmu.rb(HL);

                    case 7:
                        return A;

                    default:
                        throw new ArgumentOutOfRangeException("Register id", id, $"Register {id} was not within range of the r8 table");
                }
            }
        }

        public void LoadWordRegisterPair1(int regId, ushort word)
        {
            switch (regId)
            {
                case 0:
                    BC = word;
                    break;
                case 1:
                    DE = word;
                    break;
                case 2:
                    HL = word;
                    break;
                case 3:
                    SP = word;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("regId", regId, $"Register Id : {regId}");
            }
        }

        public void LoadWordRegisterPair2(int regId, ushort word)
        {
            switch (regId)
            {
                case 0:
                    BC = word;
                    break;
                case 1:
                    DE = word;
                    break;
                case 2:
                    HL = word;
                    break;
                case 3:
                    AF = word;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("regId", regId, $"Register Id : {regId}");
            }
        }

        public ushort ReadWordRegisterPair1(int regId)
        {
            return regId switch
            {
                0 => BC,
                1 => DE,
                2 => HL,
                3 => SP
            };
        }

        public ushort ReadWordRegisterPair2(int regId)
        {
            return regId switch
            {
                0 => BC,
                1 => DE,
                2 => HL,
                3 => AF
            };
        }

        public ushort ReadWordRegisterPair3(int regId)
        {
            return regId switch
            {
                0 => BC,
                1 => DE,
                2 => HL++,
                3 => HL--
            };
        }

        public byte RegisterPair2Indirect(int id)
        {
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
        }

        public void SetRegById(int id, byte val)
        {
            switch (id)
            {
                case 0:
                    B = val;
                    break;
                case 1:
                    C = val;
                    break;
                case 2:
                    D = val;
                    break;
                case 3:
                    E = val;
                    break;
                case 4:
                    H = val;
                    break;
                case 5:
                    L = val;
                    break;
                case 6:
                    WriteByte(HL, val);
                    break;
                case 7:
                    A = val;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("ID", id, $"Id was not in the range for a valid Register: {id}");
            }
        }

        byte ReadByte(ushort add)
        {
            return mmu.rb(add);
        }

        void WriteByte(ushort add, byte value)
        {
            mmu.wb(add, value);
        }

        public void SetAllRegs(byte value)
        {
            this.A = value;
            this.B = value;
            this.C = value;
            this.D = value;
            this.E = value;
            this.F = value;
            this.H = value;
            this.L = value;
        }
    }
}
