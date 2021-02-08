using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace GB.Emulator
{
    public class SoundChannel
    {
        public int FrequencyShift;
        public bool SweepAddition;
        public int SweepShift;
        public int SweepTime;
        private const int SWEEP_ADDITION = 8;
        private const uint EVEN_BUFFER = 0xfffffffe;
        private int SAMPLE_RATE = 22000;
        private DynamicSoundEffectInstance Channel_Out;

        private Queue<byte[]> OutBuffer;

        private double WaveDuty;

        private int EnvelopeSweep;

        private int FreqHi;

        private int FreqLo;

        private int Frequency;
        private float FrequencyHz;
        private bool IncreasingVolume;

        private int InitialVolume;

        private bool PlayOnce;

        private bool Restart;

        private float SoundLength;
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
                PlayOnce = (value & 0x40) > 0;
                Restart = (0x80 & value) > 0;

                if (Restart)
                {
                    FrequencyShift = Frequency;
                }
            }
        }

        public int SoundLengthWavePatternDuty
        {
            set
            {
                switch(value >> 6)
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
                SoundLength = CalcLength(value & 0x3);
            }
        }

        private float CalcLength(int lengthAsByte)
        {
            return (64f - lengthAsByte) * (1f / 256f);
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

        public SoundChannel(DynamicSoundEffectInstance soundOutput)
        {
            Channel_Out = soundOutput;
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
            if ((!PlayOnce && Channel_Out.PendingBufferCount < 4) || Restart)
            {
                Channel_Out.SubmitBuffer(CreateTone());
                Restart = false;
            }
        }
        private float CalcFreq(int registerFormattedByte)
        {
            return 131072 / (2048 - registerFormattedByte);
        }

        private byte[] CreateTone()
        {
            byte[] result;
            float freq;
            float length;
            if (SweepTime > 0 && SweepShift > 0)
            {
                SweepSet();
                freq = CalcFreq(FrequencyShift);
                length = //
            }
            else
            {
                freq = CalcFreq(Frequency);
                length = SoundLength;
            }


            int samplesPeriod = (int)(SAMPLE_RATE / freq);
            if (samplesPeriod % 2 != 0)
            {
                samplesPeriod++;
            }
            if (PlayOnce)
            {
                result = new byte[(Convert.ToInt32(SoundLength * SAMPLE_RATE) & EVEN_BUFFER)];
            }
            else
            {
                //create one second worth of buffer that perfectly fits the frequency.
                result = new byte[samplesPeriod * (SAMPLE_RATE / samplesPeriod)];
            }



            for (int i = 0; i < result.Length; i++)
            {
                //high portion of the wave
                for (int period = 0; period < samplesPeriod * WaveDuty && i < result.Length; period++)
                {
                    result[i++] = 0x00;
                    result[i++] = 0x80;
                }
                for (int period = 0; period < samplesPeriod * (1 - WaveDuty) && i < result.Length; period++)
                {
                    result[i++] = 0xff;
                    result[i++] = 0x7f;
                }

            }
            return result;
        }

        public void SweepSet()
        {
            if (SweepAddition && SweepShift !=0)
            {
                FrequencyShift += FrequencyShift >> SweepShift;
            }
            else if (SweepShift != 0)
            {
                FrequencyShift -= FrequencyShift >> SweepShift;
            }
        }
    }
}