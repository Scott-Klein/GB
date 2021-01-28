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
        Cartridge cartridge;
        CPU cpu;
        PPU pPU;
        MMU mMU;

        [SetUp]
        public void Setup()
        {
            cartridge = new Cartridge(TestCartsPaths.tetris);
            pPU = new PPU();
            mMU = new MMU(cartridge, pPU);
            cpu = new CPU(mMU);
            cpu.AF = 0xC58d;
            cpu.BC = 0xC58d;
            cpu.DE = 0xC58d;
            cpu.HL = 0xc58d;
        }

        [Test]
        public void All_registers_combine()
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

        [Test]
        public void IncReg()
        {
            cpu = CreateWithTestInstruction(0xc);
            var cached = cpu.C;
            cpu.Tick();
            Assert.That(cpu.C, Is.EqualTo(cached + 1));
            Assert.That(cpu.Zero, Is.Not.True);
        }

        [Test]
        public void BootRomInstructionsImplemented()
        {
            int count = 0;

            while (count++ < 1000 && cpu.PC <= 0x150)
            {
                cpu.Tick();
            }
            if (count == 0)
            {
                Assert.Fail("Ran out of time to execute the boot rom");
            }
            Assert.Pass();
        }

        public CPU CreateWithTestInstruction(byte op)
        {
            MMU m = new MMU(cartridge, pPU, true, 0xc);
            var cpu = new CPU(m);
            return cpu;
        }

    }
}
