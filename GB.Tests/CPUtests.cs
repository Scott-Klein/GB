using GB.Emulator;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Tests
{
    class CPUtests
    {
        CPU cpu;

        [SetUp]
        public void Setup()
        {
            cpu = new CPU();
            cpu.AF = 0xC58d;
            cpu.BC = 0xC58d;
            cpu.DE = 0xC58d;
            cpu.HL = 0xc58d;
        }

        [Test]
        public void All_registers_combines()
        {
            Assert.That(cpu.A, Is.EqualTo(0xc5));
            Assert.That(cpu.F, Is.EqualTo(0x8d));

            Assert.That(cpu.B == 0xc5);
            Assert.That(cpu.D == 0xc5);
            Assert.That(cpu.H == 0xc5);

            Assert.That(cpu.C == 0x8d);
            Assert.That(cpu.E == 0x8d);
            Assert.That(cpu.L == 0x8d);
        }

    }
}
