using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;

namespace GB.Emulator
{
    public class Sound
    {
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
        public DynamicSoundEffectInstance Channel_1;
        public DynamicSoundEffectInstance Channel_2;
        private byte[] WavePatternRam;
        private const float SAMPLE_RATE = 22000f;
        private const short SQUARE_HIGH = -32768;
        private const short SQUARE_LOW = 32767;
        public byte[] CHANNEL1 { get; set; }
        public BinaryWriter writer;
        public Queue<byte[]> CH1q;
        public Queue<byte[]> CH2q;

        public void Tick()
        {
            SubmitBuffers();

            //channel 1 repeat if no new sounds requested.
            if (((NR14 & 0x40) == 0 && Channel_1.PendingBufferCount < 2) || (NR14 & 0x80) > 0)
            {
                NR14 = (byte)(NR14 & 0x7f);
                EnQueueCh1();
            }

            if (((NR24 & 0x40) == 0 && Channel_2.PendingBufferCount < 2) || (NR24 & 0x80) > 0)
            {
                NR24 = (byte)(NR24 & 0x7f);
                EnQueueCh2();
            }

        }

        private void SubmitBuffers()
        {
            while (CH1q.Count > 0)
            {
                Channel_1.SubmitBuffer(CH1q.Dequeue());
            }
            while (CH2q.Count > 0)
            {
                Channel_2.SubmitBuffer(CH2q.Dequeue());
            }
        }

        public void EnQueueCh1()
        {
            int ch1Freq = NR13 | ((NR14 & 7) << 8);
            float freq = 131072f / (2048 - ch1Freq);// convert to hertz
            float lengthInSeconds = (0x40 & NR14) > 0 ?(64 - NR11 & 0x3f) * (1f / 256f): 0.5f;
            WaveDuty duty = (WaveDuty)(NR11 >> 6);
            if (lengthInSeconds > 0.0001)
            {
                CH1q.Enqueue(CreateTone(freq, lengthInSeconds, duty));
            }
        }
        public void EnQueueCh2()
        {
            int ch2Freq = NR23 | ((NR24 & 7) << 8);
            float freq = 131072f / (2048 - ch2Freq);// convert to hertz
            float lengthInSeconds = (0x40 & NR24) > 0 ? (64 - NR21 & 0x3f) * (1f / 256f) : 0.5f;
            WaveDuty duty = (WaveDuty)(NR21 >> 6);
            if (lengthInSeconds > 0.0001)
            {
                CH2q.Enqueue(CreateTone(freq, lengthInSeconds, duty));
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
                    highCycles = 1-lowCycles;
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
                    result[i++ % result.Length] =  0x00;
                    result[i++ % result.Length] =  0x80;
                }
                for (int period = 0; period < samplesPeriod * highCycles; period++)
                {
                    result[i++ % result.Length] =  0xff;
                    result[i++ % result.Length] =  0x7f;
                }
            }

            return result;
        }

        public Sound()
        {
            WavePatternRam = new byte[16];
            CH1q = new Queue<byte[]>();
            CH2q = new Queue<byte[]>();
        }

        public bool ReadyCh1;

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
                var a when a>= 0xff30 && a <= 0xff3f => WavePatternRam[a & 0xf]
            };
        }
    }

    public enum WaveDuty : byte
    {
        w125,
        w25,
        w50,
        w75
    }
}
