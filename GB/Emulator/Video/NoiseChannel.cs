using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator.Video
{
    public class NoiseChannel : SoundChannel
    {
        private int polynomialCounter;
        private double ratio;
        private bool NarrowBit;
        private double shiftClockFrequency;
        public int PolyNomialCounter 
        { 
            get
            {
                return polynomialCounter;
            }
            set
            {
                polynomialCounter = value;
                ratio = 0x7 & value;
                if (ratio == 0)
                {
                    ratio = 0.5;
                }
                NarrowBit = (value & 0x8) > 0;
                int scf = value >> 4;
                shiftClockFrequency = 524288.0 / ratio / (1 << (1 + scf)); 
            }
        }

        public override void Tick(long cycles = 0)
        {
            while (Restart && bytesWritten < bytesToWrite)
            {
                var noise = generator.GenerateNoise(shiftClockFrequency, NarrowBit, envelope, bytesToWrite);
                bytesWritten += noise.Length;
                Channel_Out.SubmitBuffer(noise);
            }
            if (bytesWritten >= bytesToWrite)
            {
                Restart = false;
                bytesWritten = 0;
                generator.lfsr = 0x7FFF;
            }
        }

        public NoiseChannel(DynamicSoundEffectInstance soundOutput) : base(soundOutput)
        {
            Channel_Out.Play();
        }
    }
}
