using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GB.Emulator
{
    public enum WaveDuty : byte
    {
        w125,
        w25,
        w50,
        w75
    }

    public class Sound
    {
        public DynamicSoundEffectInstance Channel_1;
        public DynamicSoundEffectInstance Channel_2;
        public Queue<byte[]> Channel1Buffer;

        public Queue<byte[]> Channel2Buffer;
        public bool ReadyCh1;

        public BinaryWriter writer;

        private const float SAMPLE_RATE = 22000f;

        private const short SQUARE_HIGH = -32768;

        private const short SQUARE_LOW = 32767;

        //Sound Channel 1
        private byte NR10; // CH1 Sweep.

        private byte NR11; // CH1 Length
        private byte NR12; // CH1 Volume
        private byte NR13; // CH1 Frequency lo (w)
        private byte NR14; // CH1 Frequency hi (w/r)

        //Sound Channel 2
        private byte NR21; // CH2 Length

        private byte NR22; // CH2 Volume
        private byte NR23; // CH2 Frequency lo (w)
        private byte NR24; // CH2 Frequency hi (w/r)

        //Sound Channel 3
        private byte NR30; // CH3 Sound on of

        private byte NR31; // CH3 Length
        private byte NR32; // CH3 Select output level
        private byte NR33; // CH3 Frequency lo
        private byte NR34; // CH3 Frequency hi

        //Sound Channel 4
        private byte NR41; // CH4 Sound Length

        private byte NR42; // CH4 Volume Envelope
        private byte NR43; // CH4 Polynomial Counter
        private byte NR44; // CH4 Counter/Consecutive

        private byte NR50; // Channel control
        private byte NR51; // Selection of sound output tuerminal

        private byte NR52; // Sound on/off
        private byte[] WavePatternRam;

        public Sound()
        {
            WavePatternRam = new byte[16];
            Channel1Buffer = new Queue<byte[]>();
            Channel2Buffer = new Queue<byte[]>();
        }

        public byte[] CHANNEL1 { get; set; }

        public byte ReadByte(ushort addr)
        {
            return addr switch
            {
                0xff10 => NR10,
                0xff11 => NR11,
                0xff12 => NR12,
                0xff13 => NR13,
                0xff14 => NR14,
                0xff16 => NR21,
                0xff17 => NR22,
                0xff18 => NR23,
                0xff19 => NR24,
                0xff1a => NR30,
                0xff1b => NR31,
                0xff1c => NR32,
                0xff1d => NR33,
                0xff1e => NR34,
                0xff20 => NR41,
                0xff21 => NR42,
                0xff22 => NR43,
                0xff23 => NR44,
                0xff24 => NR50,
                0xff25 => NR51,
                0xff26 => NR52,
                var a when a >= 0xff30 && a <= 0xff3f => WavePatternRam[a & 0xf]
            };
        }

        public void Tick()
        {
            //If the initial bit is set, bit 7 of the control/freq register
            // NR14, a whole new sound should be played immediately.
            if ((NR14 & 0x80) > 0)
            {
                Channel_1.Stop();
                Channel_1.Play();
            }

            //whether or now it is a new sound, make a submission to the buffer if it is nearly empty
            //or submit the new tone if it's a restart bit triggered sound.
            if (((NR14 & 0x40) == 0 && Channel_1.PendingBufferCount < 2) || (NR14 & 0x80) > 0)
            {
                //unset the restart bit.
                NR14 = (byte)(NR14 & 0x7f);
                SweepHandler();
                SubmitBuffer(Channel1Buffer, NR13, NR14, NR11);
            }

            if ((NR24 & 0x80) > 0)
            {
                Channel_2.Stop();
                Channel_2.Play();
            }

            if (((NR24 & 0x40) == 0 && Channel_2.PendingBufferCount < 2) || (NR24 & 0x80) > 0)
            {
                NR24 = (byte)(NR24 & 0x7f);
                SubmitBuffer(Channel2Buffer, NR23, NR24, NR21);
            }

            SubmitBuffers();
        }

        private bool SweepHandler()
        {
            //if sweep time is set.
            if ((0x70 & NR10) > 0)
            {
                float sweepTime = (NR10 >> 4) * 7.8f;

                Stopwatch swWatch = new Stopwatch();
                swWatch.Start();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void WriteByte(ushort addr, byte value)
        {
            switch (addr)
            {
                case 0xff10:
                    NR10 = value;
                    break;

                case 0xff11:
                    NR11 = value;
                    break;

                case 0xff12:
                    NR12 = value;
                    break;

                case 0xff13:
                    NR13 = value;
                    break;

                case 0xff14:
                    NR14 = value;// we've already restarted the sound so mask it.
                    break;

                case 0xff16:
                    NR21 = value;
                    break;

                case 0xff17:
                    NR22 = value;
                    break;

                case 0xff18:
                    NR23 = value;
                    break;

                case 0xff19:
                    NR24 = value;
                    break;

                case 0xff1a:
                    NR30 = value;
                    break;

                case 0xff1b:
                    NR31 = value;
                    break;

                case 0xff1c:
                    NR32 = value;
                    break;

                case 0xff1d:
                    NR33 = value;
                    break;

                case 0xff1e:
                    NR34 = value;
                    break;

                case 0xff20:
                    NR41 = value;
                    break;

                case 0xff21:
                    NR42 = value;
                    break;

                case 0xff22:
                    NR43 = value;
                    break;

                case 0xff23:
                    NR44 = value;
                    break;

                case 0xff24:
                    NR50 = value;
                    break;

                case 0xff25:
                    NR51 = value;
                    break;

                case 0xff26:
                    NR52 = value;
                    break;

                case var a when a >= 0xff30 && a <= 0xff3f:
                    this.WavePatternRam[a & 0xf] = value;
                    break;
            }
        }

        private byte[] CreateTone(float freq, float length, WaveDuty waveDuty = WaveDuty.w50)
        {
            byte[] result = new byte[(Convert.ToInt32(length * SAMPLE_RATE)) & 0xfffffffe];
            float samplesPeriod = SAMPLE_RATE / freq;

            float lowCycles;
            float highCycles;

            switch (waveDuty)
            {
                case WaveDuty.w125:
                    lowCycles = 0.125f;
                    highCycles = 1 - lowCycles;
                    break;

                case WaveDuty.w25:
                    lowCycles = 0.25f;
                    highCycles = 1 - lowCycles;
                    break;

                case WaveDuty.w50:
                    lowCycles = 0.5f;
                    highCycles = 1 - lowCycles;
                    break;

                case WaveDuty.w75:
                    lowCycles = 0.75f;
                    highCycles = 1 - lowCycles;
                    break;

                default:
                    lowCycles = 0.5f;
                    highCycles = 0.5f;
                    break;
            }

            for (int i = 0; i < result.Length;)
            {
                if (i % 2 != 0)
                {
                    continue;
                }

                for (int period = 0; period < samplesPeriod * lowCycles; period++)
                {
                    result[i++ % result.Length] = 0x00;
                    result[i++ % result.Length] = 0x80;
                }
                for (int period = 0; period < samplesPeriod * highCycles; period++)
                {
                    result[i++ % result.Length] = 0xff;
                    result[i++ % result.Length] = 0x7f;
                }
            }

            return result;
        }

        private void SubmitBuffer(Queue<byte[]> channelBuffer, byte freqByte, byte controlByte, byte lengthWavePatternByte)
        {
            int chFreq = freqByte | ((controlByte & 7) << 8);
            float freq = 131072f / (2048 - chFreq);// convert to hertz
            float lengthSeconds = (0x40 & controlByte) > 0 ? (64 - lengthWavePatternByte & 0x3f) * (1f / 256f) : 1.0f;
            WaveDuty duty = (WaveDuty)(lengthWavePatternByte >> 6);
            if (lengthSeconds > 0.001)
            {
                channelBuffer.Enqueue(CreateTone(freq, lengthSeconds, duty));
            }
        }
        private void SubmitBuffers()
        {
            while (Channel1Buffer.Count > 0)
            {
                Channel_1.SubmitBuffer(Channel1Buffer.Dequeue());
            }
            while (Channel2Buffer.Count > 0)
            {
                Channel_2.SubmitBuffer(Channel2Buffer.Dequeue());
            }
        }
    }
}