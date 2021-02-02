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
        }


        /// <summary>
        /// All forms of instruction 0x06, 0x16, 0x26, 0x36
        /// Compute correctly.
        /// </summary>
        [Test]
        public void LDxd8()
        {
            mmu.WriteWord(0xc000, 0xffff);
            mmu.WriteWord(0xc002, 0xffff);
            mmu.WriteWord(0xc006, 0xffff);
            regs.PC = 0xc000;

            cpu.Tick(0x06);
            Assert.That(regs.B, Is.EqualTo(0xff));

            cpu.Tick(0x16);
            Assert.That(regs.D, Is.EqualTo(0xff));

            cpu.Tick(0x26);
            Assert.That(regs.H, Is.EqualTo(0xff));

            regs.HL = 0xc006;
            Assert.That(regs.GetRegById(6), Is.EqualTo(0xff));
        }

        [Test]
        public void RLC()
        {
            cpu = new CPU(mmu, regs);
            regs.PC = 0xc000;
            regs.SetAllRegs(0x85); //put the test data into memory.
            for (int i = 0; i < 8; i++)
            {
                mmu.wb((ushort)(0xc000 + i), (byte)i); // push the RLC instructions into memory.
            }

            cpu.Tick(0xcb);
            Assert.That(regs.B, Is.EqualTo(0xb));
            cpu.Tick(0xcb);
            Assert.That(regs.C, Is.EqualTo(0xb));
            cpu.Tick(0xcb);
            Assert.That(regs.D, Is.EqualTo(0xb));
            cpu.Tick(0xcb);
            Assert.That(regs.E, Is.EqualTo(0xb));
            cpu.Tick(0xcb);
            Assert.That(regs.H, Is.EqualTo(0xb));
            cpu.Tick(0xcb);
            Assert.That(regs.L, Is.EqualTo(0xb));

            //the HL register
            mmu.wb(0xc150, 0x85);
            regs.HL = 0xc150;

            cpu.Tick(0xcb);
            Assert.That(mmu.rb(0xc150), Is.EqualTo(0xb));

            //Finally a register.
            cpu.Tick(0xcb);
            Assert.That(regs.A, Is.EqualTo(0xb));
        }

        [Test]
        public void RRC()
        {
            cpu = new CPU(mmu, regs);
            regs.PC = 0xc000;
            regs.B = 0x85;
            mmu.wb((ushort)0xc000, 0x8);
            cpu.Tick(0xcb);
            Assert.That(regs.B, Is.EqualTo(0xc2));
        }
    }
}
