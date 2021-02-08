namespace GB.Emulator
{
    public class SoundChannel
    {
        public int FrequencyOriginal;
        public int FrequencyShift;
        public bool SweepAddition;
        public int SweepShift;
        public int SweepTime;
        private const int SWEEP_ADDITION = 8;

        private WaveDuty duty;

        private int EnvelopeSweep;

        private int FreqHi;

        private int FreqLo;

        private int Frequency;

        private bool IncreasingVolume;

        private int InitialVolume;

        private bool PlayOnce;

        private bool Restart;

        private float SoundLength;

        public SoundChannel()
        {
        }

        public int FrequencyLo
        {
            set
            {
                FreqLo = value;
                Frequency = FreqLo | FreqHi;
            }
        }

        public int FrequncyHi
        {
            set
            {
                FreqHi = (value & 0x7) << 8;
                Frequency = FreqLo | FreqHi;
                PlayOnce = (value & 0x40) > 0;
                Restart = (0x80 & value) > 0;
            }
        }

        public int SoundLengthWavePatternDuty
        {
            set
            {
                duty = (WaveDuty)(value >> 6);
                SoundLength = (64f - (value & 0x3f)) * (1f / 256f);
            }
        }

        public int SweepRegister
        {
            set
            {
                SweepTime = value >> 4;
                SweepAddition = (value & SWEEP_ADDITION) > 0;
                SweepShift = 0x7 & value;
            }
        }

        public int VolumeEnvelope
        {
            set
            {
                InitialVolume = value >> 4;
                IncreasingVolume = (value & 0x8) > 0;
                EnvelopeSweep = value & 0x7;
            }
        }

        public void SweepSet(byte sweepReg)
        {
            FrequencyShift = FrequencyOriginal;
            if (SweepAddition)
            {
                FrequencyShift += FrequencyOriginal >> SweepShift;
            }
            else
            {
                FrequencyShift -= FrequencyOriginal >> SweepShift;
            }
            
        }
    }
}