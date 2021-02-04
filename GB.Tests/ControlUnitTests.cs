using GB.Emulator;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Tests.Processing
{
    class ControlUnitTests
    {

        [SetUp]
        public void Setup()
        {
            
            cart = new Cartridge(TestCartsPaths.tetris);
            clock = new Clock();
            ppu = new PPU(clock);
            mmu = new MMU(cart, ppu, clock);
            reg = new Registers(mmu);
            cpu = new CPU(mmu, reg);
        }
        Clock clock;
        Cartridge cart;
        PPU ppu;
        CPU cpu;
        MMU mmu;
        IRegisters reg;
        IControlUnit cu;

        [Test]
        [Ignore("Takes 5 hours to run")]
        public void FinishesBootRom()
        {
            int timeOut = 0;
            while (timeOut < 10000 && reg.PC < 0x100)
            {
                cpu.Tick();
            }
            if (timeOut == 0)
            {
                Assert.Fail();
            }
            Assert.Pass();
        }

        [Test]
        public void PushesPops()
        {
            cu = new ControlUnit(mmu, reg);
            var spStart = --reg.SP;
            cu.Push(0xfe1b);
            cu.Push(0x1f2e);

            Assert.That(cu.POP(), Is.EqualTo(0x1f2e));
            Assert.That(cu.POP(), Is.EqualTo(0xfe1b));
        }
    }
}
