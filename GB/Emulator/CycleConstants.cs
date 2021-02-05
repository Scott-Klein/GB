
namespace GB.Emulator
{
    public static class OpTiming
    {
        public const int CB = 12;
        public const int NOP = 4;
        public const int JR_Y = 4;
        public const int JR_N = 8;
        public const int LD = 4;
        public const int ARITHMETIC = 4;
        public const int ARITHMETIC_LOAD = 8;
        public const int LDH = 12;
        public const int SHIFT = 4;
        public const int LD_SP = 20;
        public const int POP = 12;
        public const int PUSH = 16;
        public const int CALL = 24;
        public const int NO_CALL = 16;
        public const int RST = 16;
        public const int RET = 16;
        public const int RET_C = 12;
        public const int NO_RET = 8;
        public const int JP = 16;
        public const int NO_JP = 12;
        public const int EI = 4;
        public const int LD_WORD = 12;
        public const int STORE_BYTE = 8;
        public const int INC_WORD_REG = 8;
        public const int ADD_SP = 16;
    }
}