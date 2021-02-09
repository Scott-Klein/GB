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
        public DynamicSoundEffectInstance CH1 
        {  
            set
            {
                Channel_One = new SoundChannel(value);
            }
        }

        public DynamicSoundEffectInstance CH2 
        {  
            set
            {
                Channel_Two = new SoundChannel(value);
            }
        }

        private const float SAMPLE_RATE = 22000f;

        private const short SQUARE_HIGH = -32768;

        private const short SQUARE_LOW = 32767;

        private const int SOUND_ONE_ON_FLAG = 0x01;
        private const int SOUND_TWO_ON_FLAG = 0x02;
        private const int SOUND_THREE_ON_FLAG = 0x4;
        private const int SOUND_FOUR_ON_FLAG = 0x8;
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

        private SoundChannel Channel_One;
        private SoundChannel Channel_Two;

        public Sound()
        {
            WavePatternRam = new byte[16];
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
                var a when a >= 0xff30 && a <= 0xff3f => WavePatternRam[a & 0xf]
            };
        }

        public void Tick()
        {
            if (NR52 != 0)
            {
                Channel_One.Tick();
                Channel_Two.Tick();
            }
            NR52 = Channel_One.ChannelOn ? (byte)(NR52 | SOUND_ONE_ON_FLAG) : (byte)(NR52 & (~SOUND_ONE_ON_FLAG));
        }

        public void WriteByte(ushort addr, byte value)
        {
            switch (addr)
            {
                case 0xff10:
                    NR10 = value;
                    Channel_One.SweepRegister = value;
                    break;

                case 0xff11:
                    NR11 = value;
                    Channel_One.SoundLengthWavePatternDuty = value;
                    break;

                case 0xff12:
                    NR12 = value;
                    Channel_One.VolumeEnvelope = value;
                    break;

                case 0xff13:
                    NR13 = value;
                    Channel_One.FrequencyLo = value;
                    break;

                case 0xff14:
                    NR14 = (byte)(0x7f & value);
                    Channel_One.FrequncyHi = value;
                    break;

                case 0xff16:
                    Channel_Two.SoundLengthWavePatternDuty = value;
                    NR21 = value;
                    break;

                case 0xff17:
                    NR22 = value;
                    Channel_Two.VolumeEnvelope = value;
                    break;

                case 0xff18:
                    NR23 = value;
                    Channel_Two.FrequencyLo = value;
                    break;

                case 0xff19:
                    NR24 = (byte)(0x7f & value);
                    Channel_Two.FrequncyHi = value;
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

    }
}