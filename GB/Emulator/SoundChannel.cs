using Microsoft.Xna.Framework.Audio;

namespace GB.Emulator
{
    public class SoundChannel
    {
        public int FrequencyLo
        {
            set
            {
                FreqLo = value;
            }
        }

        public int FrequncyHi
        {
            set
            {
                FreqHi = (value & 0x7) << 8;
                Frequency = FreqLo | FreqHi;
                FrequencyHz = CalcFreq(Frequency);
                PlayOnce = (value & 0x40) > 0;
                Restart = (0x80 & value) > 0;
                sweep.Frequency = Frequency;
            }
        }

        public int SoundLengthWavePatternDuty
        {
            set
            {
                switch (value >> 6)
                {
                    case 0:
                        WaveDuty = 0.125;
                        break;

                    case 1:
                        WaveDuty = 0.25;
                        break;

                    case 2:
                        WaveDuty = 0.5;
                        break;

                    case 3:
                        WaveDuty = 0.75;
                        break;
                }
                SoundLength = CalcLength(value & 0x3f);
            }
        }

        public int SweepRegister
        {
            set
            {
                sweep.SweepTime = value >> 4;
                sweep.Subtraction = (value & SWEEP_ADDITION) > 0;
                sweep.SweepShift = 0x7 & value;
            }
        }

        public int VolumeEnvelope
        {
            set
            {
                envelope.Sweep = value & 0x7;
                envelope.Increasing = (value & 0x8) > 0;
                envelope.InitialVolume = value >> 4;
            }
        }

        private const int SWEEP_ADDITION = 8;
        private DynamicSoundEffectInstance Channel_Out;
        private float FrequencyHz;
        private int FreqHi;
        private int FreqLo;
        private int Frequency;
        private bool PlayOnce;
        private bool Restart;
        private int SAMPLE_RATE = 44000;
        private float SoundLength;
        private Envelope envelope;
        private double WaveDuty;
        private Sweep sweep;

        private ToneGenerator generator;

        public SoundChannel(DynamicSoundEffectInstance soundOutput)
        {
            Channel_Out = soundOutput;
            generator = new ToneGenerator(SAMPLE_RATE);
        }

        public void Tick()
        {
            if (Restart)
            {
                //Stopping clears the buffer.
                Channel_Out.Stop();
                Channel_Out.Play();
            }

            //if looping and the buffer is empty, or starting a new sound.
            if (Play())
            {
                Channel_Out.SubmitBuffer(CreateTone());
                Restart = false;
            }
        }

        private byte[] CreateTone()
        {
            if (envelope.Sweep > 0 || sweep.SweepEnable())
            {
                PlayOnce = true;
                return generator.GenerateTone(envelope, sweep);
                return generator.GenerateTone(FrequencyHz, SoundLength, envelope);
            }
            else
            {
                return generator.GenerateTone(FrequencyHz, SoundLength, envelope.InitialVolume * 1000);
            }
        }

        private float CalcFreq(int registerFormattedByte)
        {
            return 131072 / (2048 - registerFormattedByte);
        }

        private float CalcLength(int lengthAsByte)
        {
            return (64f - lengthAsByte) * (1f / 256f);
        }

        private bool Play()
        {
            if (FrequencyHz < 72)
            {
                Restart = false;
                return false;

            }
            if(!Restart && PlayOnce)
            {
                return false;
            }
            if (Channel_Out.PendingBufferCount > 2)
            {
                return false;
            }
            if (!PlayOnce)
            {
                return true;
            }
            return true;
        }

    }
}