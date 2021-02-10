using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator
{
    public struct Envelope
    {
        public int InitialVolume;
        public bool Increasing;
        public int Sweep;

        public double GetStepSeconds()
        {
            return Sweep * (1.0 / 64.0);
        }
    }

    public struct Sweep
    {
        public int SweepTime;
        public bool Subtraction;
        public int SweepShift;
        public int Frequency;
        public bool SweepEnable()
        {
            if (SweepTime == 0 || SweepShift == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool DisableSound;
        public void SweepSet()
        {
            if (!Subtraction && SweepShift != 0)
            {
                Frequency += Frequency >> SweepShift;
            }
            else if (SweepShift != 0)
            {
                Frequency -= Frequency >> SweepShift;
            }
            if (Frequency > 0x7ff)
            {
                // If the value of frequency is greater than 2047, no sound is played,
                // and the channel 1 flag of NR52 is reset.
                //ChannelOn = false;
                DisableSound = true;
            }
        }
    }

}
