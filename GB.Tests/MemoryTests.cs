using GB.Emulator;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Tests
{
    class MemoryTests
    {

        [Test]
        public void ReadWriteWord()
        {
            var mmu = new MMU();

            ushort testword = 0xf1b2;
            mmu.WriteWord(0xff80, testword);

            Assert.That(mmu.rw(0xff80), Is.EqualTo(testword));
        }
    }
}
