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

        public bool ChannelOn { get; set; }

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
            Play = false;
        }
        private bool Play;
        public void Tick()
        {
            if (Restart)
            {
                //Stopping clears the buffer.
                Channel_Out.Stop();
                Channel_Out.Play();
                Play = true;
                ChannelOn = true;
            }

            //if looping and the buffer is empty, or starting a new sound.
            if (((!PlayOnce && Channel_Out.PendingBufferCount < 4) || Restart) && Play)
            {
                Channel_Out.SubmitBuffer(CreateTone());
                Restart = false;
            }
        }

        private float CalcFreq(int registerFormattedByte)
        {
            return 131072 / (2048 - registerFormattedByte);
        }

        private byte[] CreateSweep()
        {
            
            // Get the sweep time in seconds
            float length = SweepTime * 0.0078f;

            //the byte[] needs to be twice as long as it's an array of 16bit digits, each representing a sample.
            //To get the correct array size multiply the sample rate by 2 so that the sound lasts for the period
            // that it is meant to cover.
            byte[] sweepTone = new byte[(int)(length * (SAMPLE_RATE * 2))];
            // Convert the frequency to Hz
            // (Frequency stored in bytes are not in herts and need to be converted.
            float freq = CalcFreq(FrequencyShift);
            if (freq > 0x7ff)
            {
                // If the value of frequency is greater than 2047, no sound is played,
                // and the channel 1 flag of NR52 is reset.
                ChannelOn = false;
                return sweepTone;
            }
            // calculate lamda for the current frequency, 
            // lambda will be in bytes as the sample rate
            int samplesPeriod = (int)(SAMPLE_RATE / freq);

            int periodsTotal = sweepTone.Length / samplesPeriod;
            for (int period = 0; period < periodsTotal; period++)
            {
                for (int i = 0; i < samplesPeriod * WaveDuty;)
                {
                    sweepTone[(period * samplesPeriod) + i++] = 0x00;
                    sweepTone[(period * samplesPeriod) + i++] = 0x80;
                }
                for (int i = 0; i < samplesPeriod * (1 - WaveDuty);)
                {
                    sweepTone[(period * samplesPeriod) + i++] = 0xff;
                    sweepTone[(period * samplesPeriod) + i++] = 0x7f;
                }
            }
            // Get the current tone shifted
            SweepSet();
            return sweepTone;
        }

        private byte[] CreateTone()
        {
            byte[] result;
            float freq;
            float length;
            if (SweepTime > 0 && SweepShift > 0)
            {
                return CreateSweep();
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

            for (int i = 0; i < result.Length;)
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
            if (SweepAddition && SweepShift != 0)
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