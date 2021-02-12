using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GB.Emulator
{
    public class SoundChannel
    {
        private Random rand;
        public byte[] WavePatternRam;
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
        public int Ch3SoundLength
        {
            set
            {
                SoundLength = (256f - value) * (1f / 256f);
            }
        }
        public int WaveDataVolume
        {
            set
            {
                switch (value >> 5)
                {
                    case 0:
                        waveVolume = 4;
                        break;
                    case 1:
                        waveVolume = 0;
                        break;
                    case 2:
                        waveVolume = 1;
                        break;
                    case 3:
                        waveVolume = 2;
                        break;
                }
            }
        }

        private int waveVolume;

        public int Ch3HighData
        {
            set
            {
                int oldFrequency = Frequency;
                FreqHi = (value & 0x7) << 8;
                Frequency = FreqLo | FreqHi;
                FrequencyHz = 65536f / (2048 - Frequency);
                PlayOnce = (value & 0x40) > 0;
                Restart = (0x80 & value) > 0;
                sweep.Frequency = Frequency;
                period = CLOCK_RATE / FrequencyHz;
                samplesPerPeriod = SAMPLE_RATE / FrequencyHz;
                FrequencyUpdated = oldFrequency != Frequency;
                if (Restart)
                {
                    TriggerWavePlay();
                }
            }
        }

        

        private bool FrequencyUpdated;
        private double samplesPerPeriod;
        private double period;
        private int currentPeriod;
        private const int CLOCK_RATE = 4194304;
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
        public bool ChannelOn { get; set; }
        private ToneGenerator generator;
        public bool ChannelThree;

        public SoundChannel(DynamicSoundEffectInstance soundOutput)
        {
            Channel_Out = soundOutput;
            generator = new ToneGenerator(SAMPLE_RATE);
            WavePatternRam = new byte[16];
            waveBuffer = new short[880];
            rand = new Random();
            pendingSamples = new List<short>();
        }

        short[] waveBuffer;
        byte[] bBuffer;
        int sample;
        private int wavePatternIndex;
        private double samplePeriodIndex;
        private List<short> pendingSamples;
        public void Tick(long cycles = 0)
        {
            currentPeriod += (int)cycles;
            if (Restart && !ChannelThree)
            {
                //Stopping clears the buffer.
                Channel_Out.Stop();
                Channel_Out.Play();
            }

            //if looping and the buffer is empty, or starting a new sound.
            if (Play() && !ChannelThree)
            {
                Channel_Out.SubmitBuffer(CreateTone());
                Restart = false;
            }
            else if (Play() && ChannelThree && currentPeriod > period)
            {
                //int sampleFactor = MultiplySamples();
                //
                ////take a sample
                //waveBuffer = new short[50* pendingSamples.Count * sampleFactor];
                //for (int i = 0; i <50* pendingSamples.Count; i++)
                //{
                //    for (int j = 0; j < sampleFactor; j++)
                //    {
                //        waveBuffer[sampleFactor * i + j] = pendingSamples[i % pendingSamples.Count];
                //    }
                //    wavePatternIndex++;
                //}
                //currentPeriod = 0;
                ////EaseOut();
                //
                //if (waveBuffer.Length > 0)
                //{
                //    bBuffer = waveBuffer.SelectMany(x => BitConverter.GetBytes(x)).ToArray();
                //    Channel_Out.SubmitBuffer(bBuffer);
                //    pendingSamples = new List<short>();
                //}
            }
            else if (Play() && ChannelThree)
            {
                //while(Channel_Out.PendingBufferCount < 50)
                //{
                //    Channel_Out.SubmitBuffer(bBuffer);
                //}
            }
        }

        private void TriggerWavePlay()
        {
            int sampleFactor = MultiplySamples();
            Channel_Out.Stop();
            Channel_Out.Play();
            //take a sample
            var sampleLength = Channel_Out.GetSampleSizeInBytes(TimeSpan.FromSeconds(SoundLength));
            int bytesWritten = 0;

            while (bytesWritten < sampleLength)
            {
                waveBuffer = new short[25 * pendingSamples.Count * sampleFactor];
                for (int i = 0; i < 25 * pendingSamples.Count; i++)
                {
                    for (int j = 0; j < sampleFactor; j++)
                    {
                        waveBuffer[sampleFactor * i + j] = pendingSamples[i % pendingSamples.Count];
                    }
                    wavePatternIndex++;
                }

                bBuffer = waveBuffer.SelectMany(x => BitConverter.GetBytes(x)).ToArray();
                Channel_Out.SubmitBuffer(bBuffer);
                bytesWritten += bBuffer.Length;
            }
            pendingSamples = new List<short>();
        }

        private int MultiplySamples()
        {
            int sampleFactor = 2;
            if (FrequencyUpdated)
            {
                while (sampleFactor * FrequencyHz * 32 < 8000)
                {
                    sampleFactor++;
                }
                while(sampleFactor * FrequencyHz * 32 > 44000)
                {
                    sampleFactor--;
                }
                this.Channel_Out.Dispose();
                this.Channel_Out = new DynamicSoundEffectInstance((int)(32 * FrequencyHz * sampleFactor), AudioChannels.Mono);
                this.Channel_Out.Play();
                FrequencyUpdated = false;
            }

            return sampleFactor;
        }

        internal void WriteToWaveBuffer(byte value)
        {
            short first = (short)((value & 0xf0) >> 4);
            short second = (short)(value & 0xf);

            first = NormaliseValue(first);
            second = NormaliseValue(second);

            pendingSamples.Add((short)(first - 0x1e00));
            pendingSamples.Add((short)(second - 0x1e00));

        }

        private short NormaliseValue(short value)
        {
            value >>= waveVolume;
            value <<= 10;
            return value;
        }


        void EaseOut()
        {
            var tenth = 1.0;
            for (int i = waveBuffer.Length-20; i < waveBuffer.Length; i++)
            {
                waveBuffer[i] = (short)(waveBuffer[i] * tenth);
                tenth -= 0.05;
            }
        }

        int bufferSubmits;
        private byte[] WaveSampleToBytes(List<short> samples)
        {
            var result = new byte[samples.Count * 2];
            int resultCounter = 0;
            for (int i = 1; i < samples.Count; i++)
            {
                var s1 = samples[i - 1];
                var s2 = samples[i];

                short[] interpolatedSamples = InterpolateSamples(FrequencyHz * 32, SAMPLE_RATE, s1, s2);
                byte[] intSamplesAsBytes = interpolatedSamples.SelectMany(x => BitConverter.GetBytes(x)).ToArray();

                if (resultCounter + intSamplesAsBytes.Length > result.Length)
                {
                    Array.Copy(intSamplesAsBytes, 0, result, resultCounter, result.Length - resultCounter);
                    return result;
                }

                Array.Copy(intSamplesAsBytes, 0, result, resultCounter, intSamplesAsBytes.Length);
                resultCounter += intSamplesAsBytes.Length;
            }


            return result;
        }

        private short[] InterpolateSamples(double inSampleRate, double outSampleRate, short v1, short v2)
        {
            double samplesNeeded = outSampleRate / inSampleRate;
            double extraChance = samplesNeeded - (int)samplesNeeded;
            int samples = rand.NextDouble() < extraChance ? (int)samplesNeeded + 1 : (int)samplesNeeded;
            short diff = (short)Math.Abs(v2 - v1);
            short[] outResult = new short[samples];
            Array.Fill<short>(outResult, v1);
            for (int i = 0; i < outResult.Length; i++)
            {
                if (v2 > v1)
                {
                    outResult[i] += (short)((diff / samples) * i);
                }
                else
                {
                    outResult[i] -= (short)((diff / samples) * i);
                }
            }

            return outResult;
        }

        private byte[] CreateTone()
        {
            if (envelope.Sweep > 0 || sweep.SweepEnable())
            {
                PlayOnce = true;
                return generator.GenerateTone(envelope, sweep);
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