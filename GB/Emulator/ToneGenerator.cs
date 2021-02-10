﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator
{
    public class ToneGenerator
    {
        //private const double EVELOPE_STEP_SAMPLES = (343.75);

        public int SAMPLE_RATE { get; set; }
        public const int AMPLITUDE_STEPS = 1000;
        private long carry;

        double timePeriod;
        double frequency;
        private readonly double sweepSeconds;

        public ToneGenerator(int sampleRate)
        {
            SAMPLE_RATE = sampleRate;
            carry = 0;
            sweepSeconds = 1.0 / 128.0 * sampleRate;
        }

        public byte[] GenerateTone(double frequency, double length, int amp = 16000)
        {
            short[] result = new short[LengthInSamples(length)];
            timePeriod = (Math.PI * 2 * frequency) / (SAMPLE_RATE);
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToInt16(Math.Clamp((amp * 15) * Math.Sin(timePeriod * (i + carry)), 0 - amp, amp));
                
            }
            var buffer = result.SelectMany(x => BitConverter.GetBytes(x)).ToArray();
            carry = result.Length + carry;
            return buffer;
        }

        public byte[] GenerateTone(double frequency, double length, Envelope envelope)
        {
            var stepLength = envelope.GetStepSeconds();
            //byte[] result = new byte[LengthInSamples(length)*2];
            byte[] result = new byte[LengthInSamples(2f)];
            byte[] outBuffer;

            int resultIndex = 0;

            while ((envelope.Increasing && envelope.InitialVolume != 0xf) || (!envelope.Increasing && envelope.InitialVolume != 0) && resultIndex < result.Length)
            {
                if (envelope.Increasing)
                {
                    outBuffer = GenerateTone(frequency, stepLength, AMPLITUDE_STEPS * envelope.InitialVolume++);
                }
                else
                {
                    outBuffer = GenerateTone(frequency, stepLength, AMPLITUDE_STEPS * envelope.InitialVolume--);
                }
                if (resultIndex + outBuffer.Length > result.Length)
                {
                    Array.Copy(outBuffer, 0, result, resultIndex, result.Length - resultIndex);
                    return result; //array is full, return it.
                }
                Array.Copy(outBuffer, 0, result, resultIndex, outBuffer.Length);
                resultIndex += outBuffer.Length;
            }
            
            return result;
        }

        public byte[] GenerateTone(Envelope envelope, Sweep sweep)
        {
            double timeResolution = 1.0 / 128.0;
            int timeSteps = 0;
            byte[] result = new byte[LengthInSamples(2f)];

            byte[] outBuffer;
            double frequency = CalcFreq(sweep.Frequency);
            int resultIndex = 0;

            while (true)
            {
                outBuffer = GenerateTone(frequency, timeResolution, AMPLITUDE_STEPS * envelope.InitialVolume);
                timeSteps++;

                if (envelope.Sweep > 0 && timeSteps % (envelope.Sweep*2) == 0)
                {
                    if (envelope.Increasing)
                    {
                        envelope.InitialVolume++;
                    }
                    else
                    {
                        envelope.InitialVolume--;
                    }
                }
                if (sweep.SweepEnable() && timeSteps % sweep.SweepTime == 0)
                {
                    sweep.SweepSet();
                    frequency = CalcFreq(sweep.Frequency);
                }
                if (resultIndex + outBuffer.Length > result.Length)
                {
                    Array.Copy(outBuffer, 0, result, resultIndex, result.Length - resultIndex);
                    return result; //array is full, return it.
                }
                Array.Copy(outBuffer, 0, result, resultIndex, outBuffer.Length);
                resultIndex += outBuffer.Length;
                if ((envelope.Increasing && envelope.InitialVolume == 0xf )||(!envelope.Increasing && envelope.InitialVolume == 0x00))
                {
                    return result;
                }
            }
        }

        private double CalcFreq(int registerFormattedByte)
        {
            return 131072.0 / (2048.0 - registerFormattedByte);
        }
        private int LengthInSamples(double length)
        {
            return Convert.ToInt32(length * SAMPLE_RATE);
        }

        int FrequencyShift;
    }
}
