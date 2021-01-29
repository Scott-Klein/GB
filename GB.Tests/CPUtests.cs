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
        IRegisters regs;
        MMU mmu;
        [SetUp]
        public void SetUp()
        {
            mmu = new MMU();
            regs = new Registers(mmu);
            cpu = new CPU(mmu, regs);
            regs.SP--;
        }

        [Test]
        public void POPtoWordRegister()
        {
            ushort testword1 = 0x1234;
            ushort testword2 = 0x5678;
            ushort testword3 = 0x9abc;
            ushort testword4 = 0xdef0;
            regs.BC = testword1;
            cpu.Tick(0xc5);
            regs.BC = testword2;
            cpu.Tick(0xc5);
            regs.BC = testword3;
            cpu.Tick(0xc5);
            regs.BC = testword4;
            cpu.Tick(0xc5);

            cpu.Tick(0xc1);
            cpu.Tick(0xd1);
            cpu.Tick(0xe1);
            cpu.Tick(0xf1);

            Assert.That(regs.BC, Is.EqualTo(testword4));
            Assert.That(regs.DE, Is.EqualTo(testword3));
            Assert.That(regs.HL, Is.EqualTo(testword2));
            Assert.That(regs.AF, Is.EqualTo(testword1));
        }
    }
}
