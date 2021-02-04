using GB.Emulator;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Tests
{
    class ClockTests
    {
        private const ushort TAC_ADDRESS = 0xff07;
        private const ushort DIV_ADDRESS = 0xff04;
        private const ushort TMA_ADDRESS = 0xff06;
        private const ushort TIMA_ADDRESS = 0xff05;
        private const byte TIMER_ENABLE = 0b100;
        private const byte K1024 = 0b00;
        private const byte K16 = 0b01;
        private const byte K64 = 0b10;
        private const byte K256 = 0b11;
        private const int TIMA_OVERFLOW_CYCLES = 4;
        Clock clock;
        [SetUp]
        public void SetUp()
        {
            clock = new Clock();
        }

        //main div tick
        [Test]
        public void DivTick()
        {
            var start = clock.ReadByte(DIV_ADDRESS);
            clock.Tick(256);
            Assert.That(clock.ReadByte(DIV_ADDRESS), Is.EqualTo(start + 1));
        }

        [Test]
        public void TIMAoverflow()
        {
            byte testValue = 0xa1;
            clock.WriteByte(TMA_ADDRESS, testValue);
            clock.WriteByte(TAC_ADDRESS, TIMER_ENABLE | K16);
            while (clock.ReadByte(TIMA_ADDRESS) != 0xff)
            {
                clock.Tick(K16);
            }

            while(!clock.Overflow)
            {
                clock.Tick();
            }
            for (int i = 0; i < TIMA_OVERFLOW_CYCLES; i++)
            {
                clock.Tick();
                Assert.That(clock.ReadByte(TIMA_ADDRESS), Is.EqualTo(0));
            }
            clock.Tick();
            Assert.That(clock.ReadByte(TIMA_ADDRESS), Is.EqualTo(testValue));
        }

        //Make sure TIMA increments
        [Test]
        public void TIMAinc()
        {
            //set TIMA increment with TAC
            clock.WriteByte(0xff07, TIMER_ENABLE | K1024);
            var StartValue = clock.ReadByte(TIMA_ADDRESS);

            //increment by 1024
            clock.Tick(1024);
            clock.Tick(1024);

            var EndValue = clock.ReadByte(TIMA_ADDRESS);
            Assert.That(EndValue, Is.EqualTo(StartValue +2));

        }

        [Test]
        public void CanAbortInterrupt()
        {
            clock.WriteByte(TAC_ADDRESS, TIMER_ENABLE);
            while(!clock.Overflow)
            {
                clock.Tick();
            }

            clock.WriteByte(TIMA_ADDRESS, 0x5);

            for (int i = 0; i < 5; i++)
            {
                clock.Tick();
                Assert.That(clock.IF == 0);
            }
        }
    }
}
